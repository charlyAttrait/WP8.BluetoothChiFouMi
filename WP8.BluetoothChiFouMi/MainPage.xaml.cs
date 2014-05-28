using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using Windows.Networking.Sockets;
using Windows.Networking.Proximity;
using WP8.BluetoothChiFouMi.Resources;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Phone.Tasks;

namespace WP8.BluetoothChiFouMi
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            SystemTray.SetProgressIndicator(this, new ProgressIndicator());
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ((ViewModels.ViewModelMainPage)this.DataContext).OnNavigatedTo(null);
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            ((ViewModels.ViewModelMainPage)this.DataContext).OnNavigatingFrom(null);
        }

        private void PivotItem_Tap(object sender, GestureEventArgs e)
        {

        }
    }
}