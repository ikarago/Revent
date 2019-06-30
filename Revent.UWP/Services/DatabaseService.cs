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
            if (!CheckFileExists("db_v1.sqlite").Result)
            {
                Debug.WriteLine("DatabaseService - Create database");
                using (var db = new SQLiteConnection(DB_PATH_L))
                {
                    db.CreateTable<TemplateModel>();
                }
            }
        }

        // #TODO Import old data from roaming and put it into local

        // More database shiz. Now to check whether the dbase file already exists or not.
        private static async Task<bool> CheckFileExists(string fileName)
        {
            try
            {
                var store = await Windows.Storage.ApplicationData.Current.RoamingFolder.GetFileAsync(fileName);
                return true;
            }
            catch
            {
            }
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
