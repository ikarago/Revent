﻿using Revent.UWP.Core.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Revent.UWP.Services
{
    public static class DatabaseService
    {
        // Static properties
        private static string DB_FILE_V1 = "db_v1.sqlite";
        public static string DB_PATH_L = Path.Combine(ApplicationData.Current.LocalFolder.Path, DB_FILE_V1);
        public static string DB_PATH_R = Path.Combine(ApplicationData.Current.RoamingFolder.Path, DB_FILE_V1);

        /// <summary>
        /// Creates a new database if it doesn't exist yet
        /// </summary>
        public async static Task<bool> CreateDatabase()
        {
            // DEBUG: Amnesia mode
            //Windows.Storage.ApplicationData.Current.ClearAsync();

            // Now create the tables and such required for the dbase
            bool dbaseExists = await CheckFileExistsLocalAsync(DB_FILE_V1);

            //if (!CheckFileExistsLocalAsync(DB_FILE_V1).Result)
            if (!dbaseExists)
            {
                Debug.WriteLine("DatabaseService - Create database");
                using (var db = new SQLiteConnection(DB_PATH_L))
                {
                    db.CreateTable<TemplateModel>();
                }
            }

            return true;
        }

        /// <summary>
        /// Migrate data from the classic app (in the roaming folder) to the local folder
        /// </summary>
        /// <returns>Returns bool indicating data has been migrated in this session</returns>
        public static bool MigrateFromReventClassic()
        {
            // Check if an migration already has been done
            // This has been set to true by default. If the setting catch fails (because it doesn't exist yet) we know it can't have any migrated data as this setting only gets written at the end of the sequence
            /*bool alreadyMigrated = true;

            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            try
            {
                string alreadyMigratedString = localSettings.Values["AlreadyMigratedFromClassic"] as string;
                if (alreadyMigratedString == "true")
                {
                    alreadyMigrated = Convert.ToBoolean(alreadyMigratedString);
                }
                else { alreadyMigrated = false; }
            }
            catch { alreadyMigrated = false; }


            // If the data hasn't been migrated yet then migrate the data
            if (alreadyMigrated == false)
            {*/
            // Check if an Roaming database file exists
            try
            {
                if (CheckFileExistsRoamingAsync(DB_FILE_V1).Result)
                {
                    Debug.WriteLine("DatabaseService - Migrating database...");

                    // Get the old templates
                    ObservableCollection<TemplateModel> roamingTemplateList = new ObservableCollection<TemplateModel>();

                    // Load data from the dbase. These will be transformed into an ViewModel by the ManagerViewModel
                    using (var db = new SQLiteConnection(DB_PATH_R))
                    {
                        var query = db.Table<TemplateModel>().OrderBy(c => c.TemplateId);
                        foreach (var template in query)
                        {
                            roamingTemplateList.Add(template);
                        }
                    }


                    // Save all templates to the new local database
                    Debug.WriteLine("DatabaseService - Writing old templates to new local database...");

                    foreach (var template in roamingTemplateList)
                    {
                        DatabaseService.Write(template);
                    }


                    // Set local settings bool for successful migration to true
                    //localSettings.Values["AlreadyMigratedFromClassic"] = "true";
                    Debug.WriteLine("DatabaseService - Migration successfull! :)");


                    // #INFO: I've decided to not delete the old Roaming database by default. This is so if people are still running the old app they are still able to run it fine on older devices.
                    // Also, if they use the app on multiple devices then they're able to keep their templates on all devices instead of only 1 device.
                    return true;
                }
                else { return false; }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DatabaseService - " + ex);
                return false;
            }

            //}
            //else { return false; }
        }

        /// <summary>
        /// Checks whether the given file exists in the local folder
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static async Task<bool> CheckFileExistsLocalAsync(string fileName)
        {
            try
            {
                var store = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                return true;
            }
            catch
            { }
            return false;
        }
        /// <summary>
        /// Checks whether the given file exists in the roaming folder
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static async Task<bool> CheckFileExistsRoamingAsync(string fileName)
        {
            try
            {
                var store = await Windows.Storage.ApplicationData.Current.RoamingFolder.GetFileAsync(fileName);
                return true;
            }
            catch
            { return false; }
        }


        /// <summary>
        /// Gets the templates saved in the database
        /// </summary>
        /// <returns>Returns templates as TemplateModels in an ObservableCollection</returns>
        public static ObservableCollection<TemplateModel> GetTemplates()
        {
            Debug.WriteLine("Database Service: Getting templates");

            ObservableCollection<TemplateModel> templateList = new ObservableCollection<TemplateModel>();

            // Load data from the dbase. These will be transformed into an ViewModel by the ManagerViewModel
            using (var db = new SQLiteConnection(DB_PATH_L))
            {
                var query = db.Table<TemplateModel>().OrderBy(c => c.TemplateId);
                foreach (var template in query)
                {
                    templateList.Add(template);
                }
            }

            return templateList;
        }


        /// <summary>
        /// Writes the given TemplateModel to the database and returns the same model with an ID written to it
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public static TemplateModel Write(TemplateModel template)
        {
            Debug.WriteLine("Database Service: Saving template... ");

            using (var db = new SQLiteConnection(DB_PATH_L))
            {
                try
                {
                    var existingTemplate = (db.Table<TemplateModel>().Where(t => t.TemplateId == template.TemplateId)).SingleOrDefault();
                    if (existingTemplate != null)
                    {
                        // Update existing item in case this exists
                        existingTemplate.TemplateName = template.TemplateName;
                        existingTemplate.AppointmentAllDay = template.AppointmentAllDay;
                        existingTemplate.AppointmentDetails = template.AppointmentDetails;
                        existingTemplate.AppointmentDuration = template.AppointmentDuration;
                        existingTemplate.AppointmentLocation = template.AppointmentLocation;
                        existingTemplate.AppointmentSubject = template.AppointmentSubject;

                        int success = db.Update(existingTemplate);
                    }
                    else
                    {
                        int success = db.Insert(new TemplateModel()
                        {
                            TemplateName = template.TemplateName,
                            AppointmentAllDay = template.AppointmentAllDay,
                            AppointmentDetails = template.AppointmentDetails,
                            AppointmentDuration = template.AppointmentDuration,
                            AppointmentLocation = template.AppointmentLocation,
                            AppointmentSubject = template.AppointmentSubject
                        });

                        // Set TemplateId with the last entry in the table
                        template.TemplateId = db.Table<TemplateModel>().Last().TemplateId;
                    }

                    Debug.WriteLine("Database Service: Saving successful! :)... ");
                }
                catch
                {
                    Debug.WriteLine("Database Service: Saving failed :( ");
                }
            }

            return template;
        }

        /// <summary>
        /// Removes the given TemplateModel from the database
        /// </summary>
        /// <param name="template"></param>
        public static void Delete(TemplateModel template)
        {
            Debug.WriteLine("Database Service: Deleting Template: Id = " + template.TemplateId + " + Name = " + template.TemplateName);

            using (var db = new SQLiteConnection(DB_PATH_L))
            {
                var existingTemplate = (db.Table<TemplateModel>().Where(t => t.TemplateId == template.TemplateId)).SingleOrDefault();
                if (existingTemplate != null)
                {
                    db.RunInTransaction(() =>
                    {
                        db.Delete(existingTemplate);
                        if ((db.Table<TemplateModel>().Where(t => t.TemplateId == existingTemplate.TemplateId)).SingleOrDefault() == null)
                        {
                            Debug.WriteLine("Database Service: Item successfully deleted");
                        }
                        else
                        {
                            Debug.WriteLine("Database Service: Item was not removed :(");
                        }
                    });
                }

                // Vacuum all the stuff! (For making space for new stuff)
                try
                {
                    SQLiteCommand cmd = db.CreateCommand("VACUUM");
                    cmd.ExecuteNonQuery();
                    Debug.WriteLine("Database Service: Vacuum successful");
                }
                catch
                {
                    Debug.WriteLine("Database Service: Vacuum failed");
                }

                //ApplicationData applicationData = ApplicationData.Current;
                //applicationData.SignalDataChanged();
            }

        }

        /// <summary>
        /// Drops all data of the app (pretty much for a hard reset)
        /// </summary>
        public async static void DropAllData()
        {
            try
            {
                await Windows.Storage.ApplicationData.Current.ClearAsync();
                await Windows.Storage.ApplicationData.Current.RoamingFolder.DeleteAsync();
            }
            catch { }
        }
        /// <summary>
        /// Drops all data in the Roaming folder (in case the user doesn't want it's old info anymore.
        /// </summary>
        public async static void DropRoamingData()
        {
            try
            {
                await Windows.Storage.ApplicationData.Current.RoamingFolder.DeleteAsync();
                // To create a new RoamingFolder, just create a new file in the apps RoamingFolder and Windows will automatically create one.
            }
            catch { }
        }

        /// <summary>
        /// Optimizes database by compacting the size of the database
        /// </summary>
        public async static void OptimizeDatabase()
        {
            try
            {
                var store = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync("db_v1.sqlite");
                using (var db = new SQLiteConnection(DB_PATH_L))
                {
                    // Optimize Roaming Database-file
                    SQLiteCommand cmd = db.CreateCommand("VACUUM");
                    cmd.ExecuteNonQuery();
                }
            }
            catch { }
        }
    }
}
