using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Revent.UWP.Core.Models;
using Revent.UWP.Helpers;
using Revent.UWP.Services;
using Revent.UWP.Views.Dialogs;
using Windows.ApplicationModel.Appointments;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Revent.UWP.ViewModels
{
    public class MainViewModel : Observable
    {

        // Properties
        private ObservableCollection<TemplateModel> _templates;
        public ObservableCollection<TemplateModel> Templates
        {
            get { return _templates; }
            set { Set(ref _templates, value); }
        }

        private ObservableCollection<TemplateModel> _templatesSearchResults;
        public ObservableCollection<TemplateModel> TemplatesSearchResults
        {
            get { return _templatesSearchResults; }
            set { Set(ref _templatesSearchResults, value); }
        }

        private TemplateModel _selectedTemplate;
        public TemplateModel SelectedTemplate
        {
            get { return _selectedTemplate; }
            set { Set(ref _selectedTemplate, value); }
        }



        // Constructor
        public MainViewModel()
        {
            Initialize();
        }
        // Initialize
        private void Initialize()
        {
            // Set an empty list to populate with the Templates
            // If we don't do this and the method of getting the templates fails, the app is guaranteed to do boom
            _templates = new ObservableCollection<TemplateModel>();

            // Get the Templates from the database
            GetTemplates();

            // Make sure no template is selected
            _selectedTemplate = null;
        }



        // Commands
        private ICommand _newTemplateCommand;
        public ICommand NewTemplateCommand
        {
            get
            {
                if (_newTemplateCommand == null)
                {
                    _newTemplateCommand = new RelayCommand(
                        () =>
                        {
                            NewTemplate();
                        });
                }
                return _newTemplateCommand;
            }
        }

        private ICommand _importTemplateCommand;
        public ICommand ImportTemplateCommand
        {
            get
            {
                if (_importTemplateCommand == null)
                {
                    _importTemplateCommand = new RelayCommand(
                        () =>
                        {
                            ImportTemplate();
                        });
                }
                return _importTemplateCommand;
            }
        }

        private ICommand _editTemplateCommand;
        public ICommand EditTemplateCommand
        {
            get
            {
                if (_editTemplateCommand == null)
                {
                    _editTemplateCommand = new RelayCommand(
                        () =>
                        {
                            EditTemplate();
                        });
                }
                return _editTemplateCommand;
            }
        }

        private ICommand _deleteTemplateCommand;
        public ICommand DeleteTemplateCommand
        {
            get
            {
                if (_deleteTemplateCommand == null)
                {
                    _deleteTemplateCommand = new RelayCommand(
                        () =>
                        {
                            DeleteTemplate();
                        });
                }
                return _deleteTemplateCommand;
            }
        }

        private ICommand _openTemplateCommand;
        public ICommand OpenTemplateCommand
        {
            get
            {
                if (_openTemplateCommand == null)
                {
                    _openTemplateCommand = new RelayCommand(
                        async () =>
                        {
                            await OpenAppointmentOnCalendar();
                        });
                }
                return _openTemplateCommand;
            }
        }

        private ICommand _pinTemplateToStart;
        public ICommand PinTemplateToStart
        {
            get
            {
                if (_pinTemplateToStart == null)
                {
                    _pinTemplateToStart = new RelayCommand(
                        () =>
                        {
                            // #TODO
                        });
                }
                return _pinTemplateToStart;
            }
        }

        private ICommand _unpinTemplateToStart;
        public ICommand UnpinTemplateToStart
        {
            get
            {
                if (_unpinTemplateToStart == null)
                {
                    _unpinTemplateToStart = new RelayCommand(
                        () =>
                        {
                            // #TODO
                        });
                }
                return _unpinTemplateToStart;
            }
        }


        private ICommand _aboutCommand;
        public ICommand AboutCommand
        {
            get
            {
                if (_aboutCommand == null)
                {
                    _aboutCommand = new RelayCommand(
                        () =>
                        {
                            ShowAboutDialog();
                        });
                }
                return _aboutCommand;
            }
        }

        private ICommand _settingsCommand;
        public ICommand SettingsCommand
        {
            get
            {
                if (_settingsCommand == null)
                {
                    _settingsCommand = new RelayCommand(
                        () =>
                        {
                            ShowSettingsDialog();
                        });
                }
                return _settingsCommand;
            }
        }


        // Methods
        /// <summary>
        /// Get the saved templates from the database
        /// </summary>
        private void GetTemplates()
        {
            try
            {
                Templates = DatabaseService.GetTemplates();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MainViewModel - GetTemplates - Could not get templates. Exception in the line below:");
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Create a new Template by displaying the NewTemplateDialog and add it to the list if the user saves it
        /// </summary>
        private async void NewTemplate()
        {
            // Open the NewTemplate Dialog and get the TemplateModel made in there
            NewTemplateDialog dialog = new NewTemplateDialog();
            await dialog.ShowAsync();

            // Add this model back to the Templates List for use if the dialog wasn't cancelled
            TemplateModel model = dialog.SavedTemplate;
            if (model != null)
            {
                Templates.Add(model);
            }
        }

        private async void EditTemplate()
        {
            TemplateModel originalTemplate = SelectedTemplate;
            TemplateModel updatedTemplate;

            // Open the NewTemplate Dialog and get the TemplateModel made in there
            NewTemplateDialog dialog = new NewTemplateDialog(originalTemplate);
            await dialog.ShowAsync();
            updatedTemplate = dialog.SavedTemplate;

            // Make sure the template has been updated
            if (updatedTemplate != null)
            {
                // Remove the old template from the list
                Templates.Remove(originalTemplate);

                // Add this model back to the Templates List for use
                Templates.Add(updatedTemplate);
            }

        }


        // Open Template
        public async Task<bool> OpenAppointmentOnCalendar()
        {
            bool success = false;
            bool done = false;

            const string askForAnother = "askForAnother";
            var _localSettings = ApplicationData.Current.LocalSettings;

            if (_localSettings.Values[askForAnother].ToString() == "true")
            {
                AnotherAppointmentDialog isDoneDialog = new AnotherAppointmentDialog();
                
                do
                {
                    success = await OpenNewAppointmentOnCalendar();
                    await isDoneDialog.ShowAsync();

                    if (isDoneDialog.Result == AnotherAppointmentDialogResult.Cancel || isDoneDialog.Result == AnotherAppointmentDialogResult.DialogClosed)
                    {
                        done = true;
                    }
                }
                while (done == false);
            }
            else
            {
                success = await OpenNewAppointmentOnCalendar();
            }

            return done;
        }

        // Open the template themselves
        private async Task<bool> OpenNewAppointmentOnCalendar()
        {
            bool success = false;

            var appointment = new Appointment();

            // Set the data (if not null)
            if (SelectedTemplate.AppointmentSubject != null) { appointment.Subject = SelectedTemplate.AppointmentSubject; }
            if (SelectedTemplate.AppointmentLocation != null) { appointment.Location = SelectedTemplate.AppointmentLocation; }
            if (SelectedTemplate.AppointmentDetails != null) { appointment.Details = SelectedTemplate.AppointmentDetails; }

            Debug.WriteLine(SelectedTemplate.AppointmentDuration);

            if (SelectedTemplate.AppointmentAllDay != true)
            {
                try
                {
                    Debug.WriteLine("Duration is: " + appointment.Duration);
                    appointment.Duration = TimeSpan.FromMinutes(SelectedTemplate.AppointmentDuration);
                }
                catch
                { return success; }
            }
            else
            {
                appointment.AllDay = SelectedTemplate.AppointmentAllDay;
                DateTime midnight = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0);
                DateTimeOffset midnightOffset = new DateTimeOffset(midnight);
                appointment.StartTime = midnightOffset;
            }

            // Open it on the calendar as a new Appointment
            await Windows.ApplicationModel.Appointments.AppointmentManager.ShowEditNewAppointmentAsync(appointment);

            success = true;

            return success;
        }

        // Import Template
        private async void ImportTemplate()
        {
            // Get the user to select an .ics-file to import the information from
            StorageFile ics = null;
            ImportModel import = null;

            // Get the ICS to import:
            try
            {
                ics = await GetIcsFile();
            }
            catch
            {
                Debug.WriteLine("MainViewModel - ImportTemplate - Failed to obtain .ics-file");
            }

            // Get the info from the .ics
            import = await GetIcsDetails(ics);

            // Create a new TemplateModel infused with the imported data
            if (import != null)
            {
                TemplateModel template = new TemplateModel();

                template.AppointmentSubject = import.ImportSubject;
                template.AppointmentDetails = import.ImportDetails;
                template.AppointmentLocation = import.ImportLocation;

                // Set the ID to 0 so it'll recognise it and handle it as a Import model
                template.TemplateId = -1;


                // Open it in a TemplateEditor Dialog
                NewTemplateDialog dialog = new NewTemplateDialog(template);
                await dialog.ShowAsync();

                // Add this model back to the Templates List for use if the dialog wasn't cancelled
                TemplateModel model = dialog.SavedTemplate;
                if (model != null)
                {
                    Templates.Add(model);
                }
            }
            else
            {
                // #TODO Import unsuccessful, exit the method and display an error to the user
            }
        }

        private async Task<StorageFile> GetIcsFile()
        {
            StorageFile icsFile = null;

            // First open the FilePicker to get an ICS-file
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.FileTypeFilter.Add(".ics");

            try
            {
                // Try to get the StorageFile
                icsFile = await openPicker.PickSingleFileAsync();
            }
            catch (Exception ex)
            {
                // #TODO Give an error in case of the user cancelling it
                Debug.WriteLine("MainViewModel - GetIcsFile - Operation failed - " + ex);
            }

            return icsFile;
        }
        private async Task<ImportModel> GetIcsDetails(StorageFile ics)
        {
            ImportModel import = null;

            if (ics != null)
            {
                string s_subject = "SUMMARY:";
                string s_location = "LOCATION:";
                string s_details = "DESCRIPTION:";

                // Single use checks for screwed op ICS files
                bool b_subject = false;
                bool b_location = false;
                bool b_details = false;


                import = new ImportModel();

                IList<string> contents = await FileIO.ReadLinesAsync(ics);
                foreach (string line in contents)
                {
                    // Get Appointment Subject
                    if (line.StartsWith(s_subject))
                    {
                        if (b_subject == false)
                        {
                            import.ImportSubject = GetCleanImportString(line, s_subject);
                            b_subject = true;
                        }
                    }

                    // Get Appointment Location
                    if (line.StartsWith(s_location))
                    {
                        if (b_location == false)
                        {
                            import.ImportLocation = GetCleanImportString(line, s_location);
                            b_location = true;
                        }
                    }

                    // Get Appointment Details
                    if (line.StartsWith(s_details))
                    {
                        if (b_details == false)
                        {
                            import.ImportDetails = GetCleanImportString(line, s_details);
                            b_details = true;
                        }
                    }
                }
            }
            else
            {
                Debug.WriteLine("MainViewModel - GetIcsDetails - Inserted StorageFile is empty");

                // TODO - show error message to user
            }

            return import;
        }

        /// <summary>
        /// Removes the headers of the lines so it leaves just the info needed in the string
        /// </summary>
        /// <param name="source">The original string</param>
        /// <param name="remove">Texts that needs to be removed</param>
        /// <returns>String without the removed text</returns>
        private string GetCleanImportString(string source, string remove)
        {
            int index = source.IndexOf(remove);
            string clean = source.Remove(index, remove.Length);

            return clean;
        }


        // Delete Templates
        private void DeleteTemplate()
        {
            Debug.WriteLine("MainViewModel - Delete Template... START");

            TemplateModel template = SelectedTemplate;

            try
            {
                DatabaseService.Delete(template);
                Templates.Remove(template);
                SelectedTemplate = null;
                Debug.WriteLine("MainViewModel - Delete Template... SUCCESSFUL");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MainViewModel - Delete Template... FAILED. Error stated below:");
                Debug.WriteLine(ex.Message);
            }


        }

        // Pin Templates



        /// <summary>
        /// Opens the Settings Dialog
        /// </summary>
        private async void ShowSettingsDialog()
        {
            var dialog = new SettingsDialog();
            await dialog.ShowAsync();
        }

        /// <summary>
        /// Opens the About Dialog
        /// </summary>
        private async void ShowAboutDialog()
        {
            var dialog = new AboutDialog();
            await dialog.ShowAsync();
        }

    }
}
