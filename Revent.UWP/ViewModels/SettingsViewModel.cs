using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Toolkit.Uwp.Notifications;
using Revent.UWP.Core.Models;
using Revent.UWP.Helpers;
using Revent.UWP.Services;

using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;

namespace Revent.UWP.ViewModels
{
    // TODO WTS: Add other settings as necessary. For help see https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/pages/settings.md
    public class SettingsViewModel : Observable
    {
        // Properties
        private ElementTheme _elementTheme = ThemeSelectorService.Theme;
        public ElementTheme ElementTheme
        {
            get { return _elementTheme; }

            set { Set(ref _elementTheme, value); }
        }

        private string _versionDescription;
        public string VersionDescription
        {
            get { return _versionDescription; }

            set { Set(ref _versionDescription, value); }
        }

        private ICommand _switchThemeCommand;
        public ICommand SwitchThemeCommand
        {
            get
            {
                if (_switchThemeCommand == null)
                {
                    _switchThemeCommand = new RelayCommand<ElementTheme>(
                        async (param) =>
                        {
                            ElementTheme = param;
                            await ThemeSelectorService.SetThemeAsync(param);
                        });
                }

                return _switchThemeCommand;
            }
        }

        private bool _askForAnother;
        public bool AskForAnother
        {
            get { return _askForAnother; }
            set
            {
                if (value != _askForAnother)
                {
                    Task.Run(async () => await ApplicationData.Current.LocalSettings.SaveAsync(nameof(AskForAnother), value));
                }

                Set(ref _askForAnother, value);
            }
        }

        private bool _moreInfoOnTiles;
        public bool MoreInfoOnTiles
        {
            get { return _moreInfoOnTiles; }
            set
            {
                if (value != _moreInfoOnTiles)
                {
                    Task.Run(async () => await ApplicationData.Current.LocalSettings.SaveAsync(nameof(MoreInfoOnTiles), value));
                }

                Set(ref _moreInfoOnTiles, value);

                // If the switch is set to off then remove all the extra info on the tiles
                if (value == false)
                {
                    RemoveSecondaryTileInfo();
                }
                else
                {
                    // #TODO Fix and optimize this
                    AddSecondaryTileInfo();
                }
            }
        }


        // Constructor
        public SettingsViewModel()
        {
            Initialize();
        }

        public void Initialize()
        {
            GetSettingValues();
        }

        //public async Task InitializeAsync()
        //{
        //    VersionDescription = GetVersionDescription();
        //    await Task.CompletedTask;
        //}

        //private string GetVersionDescription()
        //{
        //    var appName = "AppDisplayName".GetLocalized();
        //    var package = Package.Current;
        //    var packageId = package.Id;
        //    var version = packageId.Version;

        //    return $"{appName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        //}

        private async void GetSettingValues()
        {
            // Reworked this with try/catches so it's not dependent on the first run bool anymore and checks stuff dynamically
            // The purpose of this is; try to read the data; if it fails set a default value on the public property so it'll be saved back to the backend settings automatically

            /// Show dialog asking to add another appointment
            try { AskForAnother = await Windows.Storage.ApplicationData.Current.LocalSettings.ReadAsync<bool>(nameof(AskForAnother)); }
            catch { AskForAnother = true; }

            /// More info on Tiles
            try { MoreInfoOnTiles = await Windows.Storage.ApplicationData.Current.LocalSettings.ReadAsync<bool>(nameof(MoreInfoOnTiles)); }
            catch { MoreInfoOnTiles = true; }
        }


        public void AddSecondaryTileInfo()
        {
            // Get a list of the Templates
            ObservableCollection<TemplateModel> templates = DatabaseService.GetTemplates();

            // Check if the template has a pinned tile
            foreach (var template in templates)
            {
                // Re-add the info to it
                if (!SecondaryTile.Exists(template.TemplateId.ToString()))
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
            // Move this code to the TileService because I'm an idiot
        }

        public void RemoveSecondaryTileInfo()
        {
            try
            {
                ObservableCollection<TemplateModel> templates = DatabaseService.GetTemplates();

                foreach (var template in templates)
                {
                    TileUpdateManager.CreateTileUpdaterForSecondaryTile(template.TemplateId.ToString()).Clear();
                    Debug.WriteLine("SettingsViewModel - Removed Secondary Tile Info for template: " + template.TemplateId.ToString());
                }
            }
            catch { Debug.WriteLine("SettingsViewModel - UH-OH, RemoveSecondaryTileInfo fucked up!"); }
        }
    }
}
