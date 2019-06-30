using Revent.UWP.Core.Models;
using Revent.UWP.Helpers;
using Revent.UWP.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Revent.UWP.Views.Dialogs
{
    public sealed partial class NewTemplateDialog : ContentDialog
    {
        // Properties
        public TemplateEditorViewModel ViewModel;
        public TemplateModel SavedTemplate;

        // Constructor
        public NewTemplateDialog(TemplateModel existModel = null)
        {
            // Create the new ViewModel, with possible existing Model as well
            ViewModel = new TemplateEditorViewModel(existModel);

            // Added because this dialog just likes to stay on Dark Theme otherwise
            RequestedTheme = (Window.Current.Content as FrameworkElement).RequestedTheme;
            this.InitializeComponent();
        }


        // Commands
        private ICommand _closeDialogCommand;
        public ICommand CloseDialogCommand
        {
            get
            {
                if (_closeDialogCommand == null)
                {
                    _closeDialogCommand = new RelayCommand(
                        () =>
                        {
                            Hide();
                        });
                }
                return _closeDialogCommand;
            }
        }

        private ICommand _saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(
                    () =>
                    {
                        Save();
                    });
                }
                return _saveCommand;
            }
        }



        // Methods
        private void Save()
        {
            SavedTemplate = ViewModel.SaveTemplate();
            Hide();
        }


    }
}
