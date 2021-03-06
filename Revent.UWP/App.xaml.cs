﻿using System;

using Revent.UWP.Core.Helpers;
using Revent.UWP.Services;

using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Revent.UWP
{
    public sealed partial class App : Application
    {
        private Lazy<ActivationService> _activationService;

        private ActivationService ActivationService
        {
            get { return _activationService.Value; }
        }

        public App()
        {
            InitializeComponent();

            EnteredBackground += App_EnteredBackground;
            Resuming += App_Resuming;

            Initialize();
        }

        private async void Initialize()
        {
            await DatabaseService.CreateDatabase();
            // Make sure this method doesn't crash the app in the long term by making it hang for some stupid reason
            //DatabaseService.MigrateFromReventClassic();
            SetSettings();

            // Deferred execution until used. Check https://msdn.microsoft.com/library/dd642331(v=vs.110).aspx for further info on Lazy<T> class.
            _activationService = new Lazy<ActivationService>(CreateActivationService);
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (!args.PrelaunchActivated)
            {
                await ActivationService.ActivateAsync(args);
            }
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }

        private ActivationService CreateActivationService()
        {
            return new ActivationService(this, typeof(Views.MainPage));
        }

        private async void App_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            var deferral = e.GetDeferral();
            await Singleton<SuspendAndResumeService>.Instance.SaveStateAsync();
            deferral.Complete();
        }

        private void App_Resuming(object sender, object e)
        {
            Singleton<SuspendAndResumeService>.Instance.ResumeApp();
        }

        private void SetSettings()
        {
            const string infoOnTiles = "MoreInfoOnTiles";
            const string askForAnother = "AskForAnother";

            // Set the local settings
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            // Now fill in the others if empty
            if (localSettings.Values[askForAnother] == null)
            {
                localSettings.Values[askForAnother] = "true";
            }
            if (localSettings.Values[infoOnTiles] == null)
            {
                localSettings.Values[infoOnTiles] = "true";
            }
        }

    }
}
