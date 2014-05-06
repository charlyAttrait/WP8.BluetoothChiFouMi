using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Storage;
using WP8.BluetoothChiFouMi.Resources;
using WP8.Core;

namespace WP8.BluetoothChiFouMi.ViewModels
{
    public class ViewModelMainPage : ObservableObject
    {
        #region Fields

        private DelegateCommand _refreshDevicesCommand;
        private DelegateCommand _connectToDeviceCommand;
        private DelegateCommand _onNavigateTo;
        private DelegateCommand _onNavigateFrom;

        private DelegateCommand _ChoiceCommand;
        private string _MyChoice;

        private DispatcherTimer _Timer;
        private DelegateCommand _StartTimer;

        ObservableCollection<PeerAppInfo> _peerApps;    // A local copy of peer app information
        StreamSocket _socket;                           // The socket object used to communicate with a peer
        string _peerName = string.Empty;                // The name of the current peer

        // Error code constants
        const uint ERR_BLUETOOTH_OFF = 0x8007048F;      // The Bluetooth radio is off
        const uint ERR_MISSING_CAPS = 0x80070005;       // A capability is missing from your WMAppManifest.xml
        const uint ERR_NOT_ADVERTISING = 0x8000000E;    // You are currently not advertising your presence using PeerFinder.Start()

        private string _TXT_PEER_Text;
        private object _LIST_SelectedItem;
        private IEnumerable _LIST_ItemsSource;
        private Boolean _isConnectionPossible;

        private ObservableCollection<string> _Records;

        #endregion

        #region Properties

        public DelegateCommand refreshDevicesCommand
        {
            get { return _refreshDevicesCommand; }
        }
        public DelegateCommand connectToDeviceCommand
        {
            get { return _connectToDeviceCommand; }
        }
        public DelegateCommand onNavigateTo
        {
            get { return _onNavigateTo; }
        }

        public DelegateCommand onNavigateFrom
        {
            get { return _onNavigateFrom; }
        }

        public DelegateCommand ChoiceCommand
        {
            get { return _ChoiceCommand; }
        }
        public string MyChoice
        {
            get { return _MyChoice; }
            set
            {
                if (_MyChoice != value)
                {
                    _MyChoice = value;
                    onPropertyChanged();
                }
            }
        }

        public DispatcherTimer Timer
        {
            get { return _Timer; }
            set
            {

            }
        }
        public DelegateCommand StartTimer
        {
            get { return _StartTimer; }
            set { _StartTimer = value; }
        }

        public string UserPseudo
        {
            get { return App.UserPseudo; }
            set
            {
                if (value != App.UserPseudo)
                {
                    App.UserPseudo = value;
                    onPropertyChanged();
                }
            }
        }

        public string TXT_PEER_Text
        {
            get { return _TXT_PEER_Text; }
            set 
            {
                if (_TXT_PEER_Text != value)
                {
                    _TXT_PEER_Text = value;
                    onPropertyChanged();
                }
            }
        }
        public object LIST_SelectedItem
        {
            get { return _LIST_SelectedItem; }
            set 
            {
                if (_LIST_SelectedItem != value)
                {
                    _LIST_SelectedItem = value;
                    onPropertyChanged();
                }
            }
        }
        public IEnumerable LIST_ItemsSource
        {
            get { return _LIST_ItemsSource; }
            set 
            {
                if (_LIST_ItemsSource != value)
                {
                    _LIST_ItemsSource = value;
                    onPropertyChanged();
                }
            }
        }
        public Boolean isConnectionPossible
        {
            get { return _isConnectionPossible; }
            set
            {
                if (_isConnectionPossible != value)
                {
                    _isConnectionPossible = value;
                    onPropertyChanged();
                }
            }
        }


        public ObservableCollection<string> Records
        {
            get { return _Records; }
            set { _Records = value; }
        }

        #endregion

        #region Constructors

        public ViewModelMainPage()
        {
            isConnectionPossible = false;

            _refreshDevicesCommand = new DelegateCommand(ExecuteRefreshDevicesCommand);
            _connectToDeviceCommand = new DelegateCommand(ExecuteConnectToDeviceCommand);

            _onNavigateTo = new DelegateCommand(OnNavigatedTo);
            _onNavigateFrom = new DelegateCommand(OnNavigatingFrom);

            _ChoiceCommand = new DelegateCommand(ExecuteChoiceCommand);

            _StartTimer = new DelegateCommand(ExecuteStartTimer);

            _Records = new ObservableCollection<string>();
        }

        #endregion

        #region Methods

        private void ExecuteRefreshDevicesCommand(object parameter)
        {
            RefreshPeerAppList();
        }

        private void ExecuteConnectToDeviceCommand(object parameter)
        {
            if (LIST_SelectedItem == null)
            {
                MessageBox.Show(AppResources.Err_NoPeer, AppResources.Err_NoConnectTitle, MessageBoxButton.OK);
                return;
            }

            // Connect to the selected peer.
            PeerAppInfo pdi = LIST_SelectedItem as PeerAppInfo;
            PeerInformation peer = pdi.PeerInfo;

            ConnectToPeer(peer);
        }


        public void OnNavigatedTo(object parameter)
        {
            // Maintain a list of peers and bind that list to the UI
            _peerApps = new ObservableCollection<PeerAppInfo>();
            LIST_ItemsSource = _peerApps;
            
            // Register for incoming connection requests
            PeerFinder.ConnectionRequested += PeerFinder_ConnectionRequested;

            // Start advertising ourselves so that our peers can find us
            PeerFinder.DisplayName = App.UserPseudo;
            PeerFinder.Start();

            RefreshPeerAppList();
        }

        public void OnNavigatingFrom(object parameter)
        {
            PeerFinder.ConnectionRequested -= PeerFinder_ConnectionRequested;

            // Cleanup before we leave
            CloseConnection(false);
        }

        public void ExecuteChoiceCommand(object parameter)
        {
            MyChoice = "/Assets/SIGLES/Left_" + parameter.ToString() + ".jpg";

            switch (parameter.ToString())
            {
                case "Chi" :

                    break;
                case "Fou" :

                    break;
                case "Mi" :

                    break;
                default :
                    break;
             }
        }

        public void ExecuteStartTimer(object parameter)
        {
            Timer = new DispatcherTimer();
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Start();
        }

        void LIST_PEERS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                isConnectionPossible = true;
            }
            else
            {
                isConnectionPossible = false;
            }
        }

        public void PeerFinder_ConnectionRequested(object sender, ConnectionRequestedEventArgs args)
        {
            try
            {
                // Ask the user if they want to accept the incoming request.
                var result = MessageBox.Show(String.Format(AppResources.Msg_ConnectionPrompt, args.PeerInformation.DisplayName)
                                                , AppResources.Msg_ConnectionPromptTitle, MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    ConnectToPeer(args.PeerInformation);
                }
                else
                {
                    // Currently no method to tell the sender that the connection was rejected.
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                CloseConnection(true);
            }
        }

        async void ConnectToPeer(PeerInformation peer)
        {
            try
            {
                _socket = await PeerFinder.ConnectAsync(peer);

                // We can preserve battery by not advertising our presence.
                PeerFinder.Stop();

                _peerName = peer.DisplayName;
            }
            catch (Exception ex)
            {
                // In this sample, we handle each exception by displaying it and
                // closing any outstanding connection. An exception can occur here if, for example, 
                // the connection was refused, the connection timeout etc.
                MessageBox.Show(ex.Message);
                CloseConnection(false);
            }
        }

        private void CloseConnection(bool continueAdvertise)
        {
            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }

            if (continueAdvertise)
            {
                // Since there is no connection, let's advertise ourselves again, so that peers can find us.
                PeerFinder.Start();
            }
            else
            {
                PeerFinder.Stop();
            }
        }

        /// <summary>
        /// Asynchronous call to re-populate the ListBox of peers.
        /// </summary>
        private async void RefreshPeerAppList()
        {
            try
            {
                StartProgress(Resources.AppResources.ResearchPeers);
                var peers = await PeerFinder.FindAllPeersAsync();

                // By clearing the backing data, we are effectively clearing the ListBox
                _peerApps.Clear();

                if (peers.Count == 0)
                {
                    TXT_PEER_Text = AppResources.Msg_NoPeers;
                }
                else
                {
                    TXT_PEER_Text = String.Format(AppResources.Msg_FoundPeers, peers.Count);
                    // Add peers to list
                    foreach (var peer in peers)
                    {
                        _peerApps.Add(new PeerAppInfo(peer));
                    }
                }
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == ERR_BLUETOOTH_OFF)
                {
                    var result = MessageBox.Show(AppResources.Err_BluetoothOff, AppResources.Err_BluetoothOffCaption, MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        ShowBluetoothControlPanel();
                    }
                }
                else if ((uint)ex.HResult == ERR_MISSING_CAPS)
                {
                    MessageBox.Show(AppResources.Err_MissingCaps);
                }
                else if ((uint)ex.HResult == ERR_NOT_ADVERTISING)
                {
                    MessageBox.Show(AppResources.Err_NotAdvertising);
                }
                else
                {
                    MessageBox.Show(ex.Message);
                }
            }
            finally
            {
                StopProgress();
            }
        }

        private void StartProgress(string message)
        {
            SystemTray.ProgressIndicator.Text = message;
            SystemTray.ProgressIndicator.IsIndeterminate = true;
            SystemTray.ProgressIndicator.IsVisible = true;
        }
        private void StopProgress()
        {
            if (SystemTray.ProgressIndicator != null)
            {
                SystemTray.ProgressIndicator.IsVisible = false;
                SystemTray.ProgressIndicator.IsIndeterminate = false;
            }
        }

        /// <summary>
        /// Open the bluetooth settings page (to activate bluetooth connection)
        /// </summary>
        private void ShowBluetoothControlPanel()
        {
            ConnectionSettingsTask connectionSettingsTask = new ConnectionSettingsTask();
            connectionSettingsTask.ConnectionSettingsType = ConnectionSettingsType.Bluetooth;
            connectionSettingsTask.Show();
        }

        private async void fillRecordsTab(string record = "")
        {
            // Get the local folder.
            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;

            if (local != null)
            {
                // OPEN / CREATE folder
                var dataFolder = await local.CreateFolderAsync("DataFolder", CreationCollisionOption.OpenIfExists);

                // OPEN / CREATE file
                var file = await dataFolder.CreateFileAsync("DataFile.txt", CreationCollisionOption.OpenIfExists);

                if (record == "") // READING ISOLATED STORAGE
                {
                    // Reading Records File
                    using (StreamReader streamReader = new StreamReader(file.Path))
                    {
                        Records.Add(streamReader.ReadLine());
                    }
                }
                else // WRITING ISOLATED STORAGE
                {
                    // Records to save
                    byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(record);

                    using (var line = await file.OpenStreamForWriteAsync())
                    {
                        line.Write(fileBytes, 0, fileBytes.Length);
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>
    ///  Class to hold all peer information
    /// </summary>
    public class PeerAppInfo
    {
        internal PeerAppInfo(PeerInformation peerInformation)
        {
            this.PeerInfo = peerInformation;
            this.DisplayName = this.PeerInfo.DisplayName;
        }

        public string DisplayName { get; private set; }
        public PeerInformation PeerInfo { get; private set; }
    }

}
