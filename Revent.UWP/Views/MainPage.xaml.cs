using System;
using System.Diagnostics;
using Revent.UWP.Core.Models;
using Revent.UWP.Services;
using Revent.UWP.ViewModels;
using Revent.UWP.Views.Dialogs;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Revent.UWP.Views
{
    public sealed partial class MainPage : Page
    {
        // Properties
        public MainViewModel ViewModel;


        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Add the check for the logo
            this.ActualThemeChanged += MainPage_ActualThemeChanged;
            CheckThemeForLogo();
        }


        // Navigation Overrides
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Check if there an parameter with a templateId has been passed when navigating to this
            if (e.Parameter != null && e.Parameter != "")
            {
                try
                {
                    // If so, set MainViewModel with the parameter so it automatically opens with the template from the secondary tile
                    ViewModel = new MainViewModel((int)e.Parameter);

                    // Hide all Main elements and display a black Grid with a progressring
                    gridSecondaryLiveTileLaunch.Visibility = Visibility.Visible;
                }
                catch { ViewModel = new MainViewModel(); }
            }
            else
            {
                ViewModel = new MainViewModel();
            }
        }


        // Methods
        private void MainPage_ActualThemeChanged(FrameworkElement sender, object args)
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

        private void LvItemTemplates_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Set the SelectedItem to the clicked item, because it'll be updated after raising this event, and therefor not operate as expected otherwise
            ViewModel.SelectedTemplate = (TemplateModel)e.ClickedItem;

            // Check if the Command can be executed and execute it
            if (ViewModel.OpenTemplateCommand.CanExecute(null))
            {
                ViewModel.OpenTemplateCommand.Execute(null);
            }
        }

        private void Grid_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            // NOTE: As this seems pretty old-fashioned to show the Flyout this way, there is a reason behind it.
            // If we use the modern method of just bundeling the ContextFlyout we won't be able to set the SelectedItem, which we need to be set in order for the Commands to work properly.

            FrameworkElement senderElement = sender as FrameworkElement;
            // Get the clicked template and set it as the selected item
            TemplateModel selectedTemplate = (TemplateModel)senderElement.DataContext;
            ViewModel.SelectedTemplate = selectedTemplate;

            // Show the Flyout
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private void Grid_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            Grid_Holding(sender, new Windows.UI.Xaml.Input.HoldingRoutedEventArgs());
        }
    }
}
