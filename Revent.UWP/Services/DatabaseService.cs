using Revent.UWP.Core.Models;
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
        public static string DB_PATH_L = Path.Combine(ApplicationData.Current.LocalFolder.Path, "db_v1.sqlite");
        public static string DB_PATH_R = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "db_v1.sqlite");

        public static void CreateDatabase()
        {
            // DEBUG: Amnesia mode
            //Windows.Storage.ApplicationData.Current.ClearAsync();

            // Now create the tables and such required for the dbase
            if (!CheckFileExistsLocalAsync("db_v1.sqlite").Result)
            {
                Debug.WriteLine("DatabaseService - Create database");
                using (var db = new SQLiteConnection(DB_PATH_L))
                {
                    db.CreateTable<TemplateModel>();
                }
            }
        }

        /// <summary>
        /// Migrate data from the classic app (in the roaming folder) to the local folder
        /// </summary>
        /// <returns>Returns bool indicating data has been migrated in this session</returns>
        public static bool MigrateFromReventClassic()
        {
            // Check if an migration already has been done
            // This has been set to true by default. If the setting catch fails (because it doesn't exist yet) we know it can't have any migrated data as this setting only gets written at the end of the sequence
            bool alreadyMigrated = true;

            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            try
            {
                string alreadyMigratedString = localSettings.Values["AlreadyMigratedFromClassic"] as string;
                if (alreadyMigratedString != "" && alreadyMigratedString != null)
                {
                    alreadyMigrated = Convert.ToBoolean(alreadyMigratedString);
                }
            }
            catch { alreadyMigrated = false; }

            // If the data hasn't been migrated yet then migrate the data
            if (alreadyMigrated == false)
            {
                // Check if an Roaming database file exists
                if (!CheckFileExistsRoamingAsync("db_v1.sqlite").Result)
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
                    Windows.Storage.ApplicationData.Current.LocalSettings.Values["AlreadyMigratedFromClassic"] = true;
                    Debug.WriteLine("DatabaseService - Migration successfull! :)");


                    // #INFO: I've decided to not delete the old Roaming database by default. This is so if people are still running the old app they are still able to run it fine on older devices.
                    // Also, if they use the app on multiple devices then they're able to keep their templates on all devices instead of only 1 device.
                    return true;
                }
                else { return false; }
            }
            else { return false; }
        }

        // More database shiz. Now to check whether the dbase file already exists or not.
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
        private static async Task<bool> CheckFileExistsRoamingAsync(string fileName)
        {
            try
            {
                var store = await Windows.Storage.ApplicationData.Current.RoamingFolder.GetFileAsync(fileName);
                return true;
            }
            catch
            { }
            return false;
        }


        // Other shiz
        // Load templates from database
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

        // Write changes to database
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

        // Delete data from database
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

        public async static void DropAllData()
        {
            try
            {
                await Windows.Storage.ApplicationData.Current.ClearAsync();
            }
            catch { }
        }

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
