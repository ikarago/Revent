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
    public sealed partial class NewTemplateDialog : ContentDialog
    {
        public NewTemplateDialog()
        {
            // Added because this dialog just likes to stay on Dark Theme otherwise
            RequestedTheme = (Window.Current.Content as FrameworkElement).RequestedTheme;
            this.InitializeComponent();
        }


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
    }
}
