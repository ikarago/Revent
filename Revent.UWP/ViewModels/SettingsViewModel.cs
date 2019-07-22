using System;
using System.Threading.Tasks;
using System.Windows.Input;

using Revent.UWP.Helpers;
using Revent.UWP.Services;

using Windows.ApplicationModel;
using Windows.Storage;
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
    }
}
