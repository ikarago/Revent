using Revent.UWP.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Revent.UWP.Views
{
    public sealed partial class SecondaryLiveTileLaunchPage : Page
    {
        // Properties
        public MainViewModel ViewModel;


        // Constructor
        public SecondaryLiveTileLaunchPage()
        {
            this.InitializeComponent();

            // Add the check for the logo
            this.ActualThemeChanged += SecondaryLiveTileLaunchPage_ActualThemeChanged;
            CheckThemeForLogo();
        }


        // Navigation Overrides
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Check if there an parameter with a templateId has been passed when navigating to this
            if (e.Parameter != null && e.Parameter != "")
            {
                // If so, set MainViewModel with the parameter so it automatically opens with the template from the secondary tile
                ViewModel = new MainViewModel((int)e.Parameter);
            }
            else
            {
                ViewModel = new MainViewModel();
            }
        }


        // Methods
        private void SecondaryLiveTileLaunchPage_ActualThemeChanged(FrameworkElement sender, object args)
        {
            CheckThemeForLogo();
        }

        private void CheckThemeForLogo()
        {
            // Change the displayed logo
            if (ActualTheme == ElementTheme.Dark)
            {
                BitmapImage image = new BitmapImage(new Uri("ms-appx:///Assets/Logo/Square44x44Logo.altform-unplated_targetsize-256.png"));
                imgAppIcon.Source = image;
            }
            else if (ActualTheme == ElementTheme.Light)
            {
                BitmapImage image = new BitmapImage(new Uri("ms-appx:///Assets/Logo/Square44x44Logo.altform-lightunplated_targetsize-256.png"));
                imgAppIcon.Source = image;
            }
        }
    }
}
