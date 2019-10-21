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

            // Set the title of the dialog
            // #TODO Set this with proper ResourceDictionaries
            if (ViewModel.ExistingTemplate == null)
            {
                // Set as New Template
                tblDialogTitle.Text = "New template";
            }
            else if (ViewModel.ExistingTemplate.TemplateId >= 1)
            {
                // Set as Edit Template
                tblDialogTitle.Text = "Edit template";
            }
            else if (ViewModel.ExistingTemplate.TemplateId == -1)
            {
                // Set as Import Template
                tblDialogTitle.Text = "Import template";
            }
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
            if (ViewModel.CheckForRequiredInput() == true)
            {
                SavedTemplate = ViewModel.SaveTemplate();
                Hide();
            }
            else
            {
                // Change border colours
                // #TODO Do this in a XAML storyboard?
                if (ViewModel.UiIsTemplateNameEmpty == true)
                {
                    txtTemplateName.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (ViewModel.UiIsSubjectEmpty == true)
                {
                    txtSubject.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
            }
        }

        private void txtTemplateName_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (ViewModel.UiIsTemplateNameEmpty == true && (txtTemplateName.Text != "" || txtTemplateName != null))
            {
                ViewModel.UiIsTemplateNameEmpty = false;
                // #TODO Reset Borders
                // TEMP, Make it green
                txtTemplateName.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Green);
                //txtTemplateName.BorderBrush = (Brush)Application.Current.Resources["TextControlBorderBrush"];

            }
        }

        private void txtSubject_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (ViewModel.UiIsSubjectEmpty == true && (txtSubject.Text != "" || txtSubject.Text != null))
            {
                ViewModel.UiIsSubjectEmpty = false;
                // #TODO Reset Borders ---> TextControlBorderBrush
                // TEMP, Make it green
                txtSubject.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Green);
                //txtSubject.BorderBrush = (SolidColorBrush)Application.Current.Resources["TextControlBorderBrush"];
            }
        }


    }
}
