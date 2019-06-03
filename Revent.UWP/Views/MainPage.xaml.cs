using System;

using Revent.UWP.ViewModels;
using Revent.UWP.Views.Dialogs;
using Windows.UI.Xaml.Controls;

namespace Revent.UWP.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public MainPage()
        {
            InitializeComponent();

        }

        private async void BtnNewTemplate_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            NewTemplateDialog temp = new NewTemplateDialog();
            await temp.ShowAsync();
        }

        private async void BtnSettings_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SettingsDialog temp = new SettingsDialog();
            await temp.ShowAsync();
        }

        private async void BtnAbout_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            AboutDialog temp = new AboutDialog();
            await temp.ShowAsync();
        }
    }
}
