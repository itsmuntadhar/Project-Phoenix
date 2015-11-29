using Microsoft.Maker.Serial;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Project_Phoenix
{
    public sealed partial class MainPage : Page
    {
        SystemNavigationManager currentView = SystemNavigationManager.GetForCurrentView();
        public MainPage()
        {
            this.InitializeComponent();
            currentView.BackRequested += CurrentView_BackRequested;
            frame.Navigated += Frame_Navigated;
            frame.Navigate(typeof(Views.MainView));
            try
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundColor = (Application.Current.Resources["SystemControlBackgroundAccentBrush"] as SolidColorBrush).Color;
                statusBar.BackgroundOpacity = 1;
                statusBar.ForegroundColor = Colors.White;
            }
            catch { }
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            currentView.AppViewBackButtonVisibility = (frame.CurrentSourcePageType != typeof(Views.MainView)) ?
                AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (frame.SourcePageType != typeof(Views.MainView))
            { frame.GoBack(); e.Handled = true; }
        }
    }
}
