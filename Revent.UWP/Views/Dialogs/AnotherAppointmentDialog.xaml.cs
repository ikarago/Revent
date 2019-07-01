using Revent.UWP.Helpers;
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
    /// <summary>
    /// Results returned from a ExitConfirmationDialog:
    /// Nothing - No action has been taken (yet) by the user.
    /// Another - User wants to use the current template to create another appointment
    /// Cancel - User wants to cancel the action they've clicked on.
    /// DialogClosed - User closed the dialog and gave no answer. Handle as 'Cancel' to the user, this option is there for debugging
    /// </summary>
    public enum AnotherAppointmentDialogResult
    {
        Nothing,
        OneMore,
        Cancel,
        DialogClosed
    }

    public sealed partial class AnotherAppointmentDialog : ContentDialog
    {
        // Properties
        public AnotherAppointmentDialogResult Result { get; set; }


        // Constructor
        public AnotherAppointmentDialog()
        {
            RequestedTheme = (Window.Current.Content as FrameworkElement).RequestedTheme;
            this.InitializeComponent();
            Result = AnotherAppointmentDialogResult.Nothing;
        }


        // Commands
        private ICommand _oneMoreCommand;
        public ICommand OneMoreCommand
        {
            get
            {
                if (_oneMoreCommand == null)
                {
                    _oneMoreCommand = new RelayCommand(
                        () =>
                        {
                            Another();
                        });
                }
                return _oneMoreCommand;
            }
        }

        private ICommand _cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(
                        () =>
                        {
                            Cancel();
                        });
                }
                return _cancelCommand;
            }
        }

        private ICommand _closeCommand;
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand(
                        () =>
                        {
                            Close();
                        });
                }
                return _closeCommand;
            }
        }




        // Methods
        private void Another()
        {
            Result = AnotherAppointmentDialogResult.OneMore;
            Hide();
        }

        private void Cancel()
        {
            Result = AnotherAppointmentDialogResult.Cancel;
            Hide();
        }

        private void Close()
        {
            Result = AnotherAppointmentDialogResult.DialogClosed;
            Hide();
        }
    }
}
