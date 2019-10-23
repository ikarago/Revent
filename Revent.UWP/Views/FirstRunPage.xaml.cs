using Revent.UWP.Services;
using Revent.UWP.Views.Dialogs;
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
using Windows.UI.Xaml.Navigation;

namespace Revent.UWP.Views
{
    public sealed partial class FirstRunPage : Page
    {
        public FirstRunPage()
        {
            this.InitializeComponent();
        }

        private async void btnCreateTemplate_Click(object sender, RoutedEventArgs e)
        {
            // Open the NewTemplate Dialog and get the TemplateModel made in there
            NewTemplateDialog dialog = new NewTemplateDialog();
            await dialog.ShowAsync();

            //Navigate to the MainPage
            NavigationService.Navigate(typeof(Views.MainPage));
        }
    }
}
