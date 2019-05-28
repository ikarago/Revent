using System;

using Revent.UWP.ViewModels;

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
    }
}
