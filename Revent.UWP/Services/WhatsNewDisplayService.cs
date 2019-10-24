using System;
using System.Threading.Tasks;

using Microsoft.Toolkit.Uwp.Helpers;

using Revent.UWP.Views.Dialogs;

namespace Revent.UWP.Services
{
    // For instructions on testing this service see https://github.com/Microsoft/WindowsTemplateStudio/tree/master/docs/features/whats-new-prompt.md
    public static class WhatsNewDisplayService
    {
        private static bool shown = false;

        internal static async Task ShowIfAppropriateAsync()
        {
            if (SystemInformation.IsAppUpdated && !shown)
            {
                shown = true;
                // #TEMP, show FirstRunPage instead
                NavigationService.Navigate(typeof(Views.FirstRunPage));

                //var dialog = new WhatsNewDialog();
                //await dialog.ShowAsync();
            }
        }
    }
}
