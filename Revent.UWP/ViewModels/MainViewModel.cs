using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Toolkit.Uwp.Notifications;
using Revent.UWP.Core.Models;
using Revent.UWP.Helpers;
using Revent.UWP.Services;
using Revent.UWP.Views.Dialogs;
using Windows.ApplicationModel.Appointments;
using Windows.Services.Store;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;

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

        // UI Properties
        private bool _uiShowImportLoading;
        public bool UiShowImportLoading
        {
            get { return _uiShowImportLoading; }
            set { Set(ref _uiShowImportLoading, value); }
        }

        private bool _uiShowImportSuccessful;
        public bool UiShowImportSuccessful
        {
            get { return _uiShowImportSuccessful; }
            set { Set(ref _uiShowImportSuccessful, value); }
        }

        // Store Properties
        private StoreContext storeContext = null;
        private bool _uiShowPurchaseButton;
        public bool UiShowPurchaseButton
        {
            get { return _uiShowPurchaseButton; }
            set { Set(ref _uiShowPurchaseButton, value); }
        }



        // Constructor
        public MainViewModel(int templateId = -1)
        {
            Initialize();

            if (templateId != -1)
            {
                // Set the selected template to the opened Template ID
                TemplateModel model = Templates.Where(t => t.TemplateId == templateId).FirstOrDefault();
                SelectedTemplate = model;

                // Open the app from a secondary tile
                OpenTemplateFromSecondaryTileAsync();
            }
        }
        // Initialize
        private async void Initialize()
        {
            // Set an empty list to populate with the Templates
            // If we don't do this and the method of getting the templates fails, the app is guaranteed to do boom
            _templates = new ObservableCollection<TemplateModel>();

            // Get the Templates from the database
            GetTemplates();

            // Make sure no template is selected
            _selectedTemplate = null;

            // Set UI elements
            UiShowImportLoading = false;
            UiShowPurchaseButton = true;

            // Get Store License info
            storeContext = StoreContext.GetDefault();
            await GetLicense();
        }


        private async void OpenTemplateFromSecondaryTileAsync()
        {
            // Open the template on the calendar
            await OpenAppointmentOnCalendar();

            // Close the app
            App.Current.Exit();
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

        private ICommand _importFromClassicCommand;
        public ICommand ImportFromClassicCommand
        {
            get
            {
                if (_importFromClassicCommand == null)
                {
                    _importFromClassicCommand = new RelayCommand(
                        () =>
                        {
                            ImportFromReventClassic();
                        });
                }
                return _importFromClassicCommand;
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

        private ICommand _pinTemplateToStartCommand;
        public ICommand PinTemplateToStartCommand
        {
            get
            {
                if (_pinTemplateToStartCommand == null)
                {
                    _pinTemplateToStartCommand = new RelayCommand(
                        () =>
                        {
                            PinToStart();
                        });
                }
                return _pinTemplateToStartCommand;
            }
        }

        private ICommand _unpinTemplateToStartCommand;
        public ICommand UnpinTemplateToStartCommand
        {
            get
            {
                if (_unpinTemplateToStartCommand == null)
                {
                    _unpinTemplateToStartCommand = new RelayCommand(
                        () =>
                        {
                            // #TODO
                        });
                }
                return _unpinTemplateToStartCommand;
            }
        }

        private ICommand _purchaseAppCommand;
        public ICommand PurchaseAppCommand
        {
            get
            {
                if(_purchaseAppCommand == null)
                {
                    _purchaseAppCommand = new RelayCommand(
                        () =>
                        {
                            PurchaseLicense();
                        });
                }
                return _purchaseAppCommand;
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

        /// <summary>
        /// Edit Template
        /// </summary>
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


        /// <summary>
        /// Open Template on Calendar
        /// </summary>
        /// <returns></returns>
        public async Task<bool> OpenAppointmentOnCalendar()
        {
            bool success = false;
            bool done = false;

            const string askForAnother = "AskForAnother";
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

        /// <summary>
        /// Import from .ics
        /// </summary>
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

        private async void ImportFromReventClassic()
        {
            // Show dialog asking the user if they wish to import their old data

            // Show loading window
            UiShowImportLoading = true;

            // Migrate from 
            bool success = DatabaseService.MigrateFromReventClassic();
            await Task.Delay(2000);

            // Hide load window and show import successful for 3 secs
            if (success == true)
            {
                UiShowImportSuccessful = true;
                UiShowImportLoading = false;
                await Task.Delay(3000);
                UiShowImportSuccessful = false;
            }
            else
            {
                // TODO Show failed message
                UiShowImportLoading = false;
                await Task.Delay(3000);
                // TODO Hide failed message
            }


            // Ask if the user wishes to keep their old data (for the classic app)


            // Refresh data
            Templates.Clear();
            GetTemplates();
        }




        /// <summary>
        /// Delete Template
        /// </summary>
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
        public void PinToStart()
        {
            PinTemplateToStart(SelectedTemplate);
        }


        private async void PinTemplateToStart(TemplateModel template)
        {
            // Check whether the tile has been pinned before...
            if (!SecondaryTile.Exists(template.TemplateId.ToString()))
            {
                // Get assets (WARNING: It'll retrieve these from the base Revent-project!)
                Uri square150x150Logo = new Uri("ms-appx:///Assets/Logo/Square150x150Logo.scale-100.png");
                Uri square44x44Logo = new Uri("ms-appx:///Assets/Logo/Square44x44Logo.scale-100.png");
                Uri square71x71Logo = new Uri("ms-appx:///Assets/Logo/Square71x71Logo.scale-100.png");
                Uri wide310x150Logo = new Uri("ms-appx:///Assets/Logo/Wide310x150Logo.scale-100.png");
                Uri square310x310Logo = new Uri("ms-appx:///Assets/Logo/Square310x310Logo.scale-100.png");

                // Set the activation arguments for the secondary tile
                string tileActivationArgs = template.TemplateId.ToString();

                // Create the secondary tile...
                SecondaryTile secTile = new SecondaryTile(template.TemplateId.ToString(),
                                                          template.TemplateName,
                                                          tileActivationArgs,
                                                          square150x150Logo,
                                                          TileSize.Square150x150);
                secTile.VisualElements.ShowNameOnSquare150x150Logo = true;
                secTile.VisualElements.ShowNameOnWide310x150Logo = true;
                secTile.VisualElements.ShowNameOnSquare310x310Logo = true;
                secTile.VisualElements.Square44x44Logo = square44x44Logo;
                secTile.VisualElements.Square71x71Logo = square71x71Logo;   // #TODO Is missing
                secTile.VisualElements.Wide310x150Logo = wide310x150Logo;
                secTile.VisualElements.Square310x310Logo = square310x310Logo;

                // And now pin it!
                await secTile.RequestCreateAsync();

                // Let's get the stuff and show it on the tile if the user wants that
                var _localSettings = ApplicationData.Current.LocalSettings;
                const string infoOnTiles = "MoreInfoOnTiles";

                if (_localSettings.Values[infoOnTiles].ToString() == "true")
                {
                    // Try to make a Tile Notification
                    try
                    {
                        TileContent content;

                        if (template.AppointmentSubject != "" && template.AppointmentLocation != "")
                        {
                            // First, let's construct the Tiles
                            content = new TileContent()
                            {
                                Visual = new TileVisual()
                                {
                                    Branding = TileBranding.NameAndLogo,
                                    // The small tile, well uh, let's keep that sucker standard
                                    // So let's edit with the Medium, Wide and Large-tiles and make these look awesome
                                    // Medium
                                    TileMedium = new TileBinding()
                                    {
                                        Content = new TileBindingContentAdaptive()
                                        {
                                            Children =
                                            {
                                                new AdaptiveText()
                                                {
                                                    HintStyle = AdaptiveTextStyle.Caption,
                                                    Text = template.AppointmentSubject
                                                },

                                                new AdaptiveText()
                                                {
                                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                                    HintWrap = true,
                                                    HintMaxLines = 3,
                                                    Text = template.AppointmentDetails
                                                }
                                            }
                                        }
                                    },
                                    // Wide
                                    TileWide = new TileBinding()
                                    {
                                        Content = new TileBindingContentAdaptive()
                                        {
                                            Children =
                                             {
                                                new AdaptiveText()
                                                {
                                                    HintStyle = AdaptiveTextStyle.Body,
                                                    Text = template.AppointmentSubject
                                                },

                                                new AdaptiveText()
                                                {
                                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                                    HintWrap = true,
                                                    HintMaxLines = 3,
                                                    Text = template.AppointmentDetails
                                                }
                                             }
                                        }
                                    },
                                    // Large
                                    TileLarge = new TileBinding()
                                    {
                                        Content = new TileBindingContentAdaptive()
                                        {
                                            Children =
                                            {
                                                new AdaptiveText()
                                                {
                                                    HintStyle = AdaptiveTextStyle.Subtitle,
                                                    Text = template.AppointmentSubject
                                                },

                                                new AdaptiveText()
                                                {
                                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                                    HintWrap = true,
                                                    Text = template.AppointmentLocation
                                                },

                                                new AdaptiveText()
                                                {
                                                    HintStyle = AdaptiveTextStyle.Caption,
                                                    HintWrap = true,
                                                    Text = template.AppointmentDetails
                                                }
                                            }
                                        }
                                    }
                                }
                            };  // END OF NORMAL-TILE

                            // Now, let's turn the code into a tile notification
                            var tileNotification = new TileNotification(content.GetXml());
                            // Now try to give it a tag, in case the tile needs to be updated
                            try { tileNotification.Tag = template.TemplateId.ToString(); }
                            catch { Debug.WriteLine("Could not give Secondary Live Tile an ID"); } // In case stuff fuck up, it won't add the ID to the Tile notification

                            // And show the new Tile Notification!
                            TileUpdateManager.CreateTileUpdaterForSecondaryTile(template.TemplateId.ToString()).Update(tileNotification);
                        }
                    }
                    catch { Debug.WriteLine("Could not create Secondary Live Tile"); }
                }
            }
        }


        // Store Stuff
        private async Task GetLicense()
        {
            StoreAppLicense license = await storeContext.GetAppLicenseAsync();
            if (license.IsActive)
            {
                if (license.IsTrial)
                {
                    UiShowPurchaseButton = true;
                }
                else
                {
                    UiShowPurchaseButton = false;
                }
            }
            else
            {
                UiShowPurchaseButton = true;
            }
        }

        private void PurchaseApp()
        {
            PurchaseLicense();
        }

        private async void PurchaseLicense()
        {
            StoreProductResult productResult = await storeContext.GetStoreProductForCurrentAppAsync();
            if (productResult.ExtendedError != null)
            {
                // An error has occurred
                return;
            }

            StoreAppLicense appLicense = await storeContext.GetAppLicenseAsync();
            if (appLicense.IsTrial)
            {
                StorePurchaseResult purchaseResult = await productResult.Product.RequestPurchaseAsync();

                // TODO: Do something with the result, like show a message

                await GetLicense();
            }
        }


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
