using System;
using Revent.UWP.Core.Models;
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
    }
}
