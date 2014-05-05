using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
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

        ObservableCollection<PeerAppInfo> _peerApps;    // A local copy of peer app information
        StreamSocket _socket;                           // The socket object used to communicate with a peer
        string _peerName = string.Empty;                // The name of the current peer

        // Error code constants
        const uint ERR_BLUETOOTH_OFF = 0x8007048F;      // The Bluetooth radio is off
        const uint ERR_MISSING_CAPS = 0x80070005;       // A capability is missing from your WMAppManifest.xml
        const uint ERR_NOT_ADVERTISING = 0x8000000E;    // You are currently not advertising your presence using PeerFinder.Start()

        private ListBox _LIST_PEERS;
        private string _TXT_PEER_Text;
        private Boolean _isConnectionPossible;

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

        public ListBox LIST_PEERS
        {
            get { return _LIST_PEERS; }
            set
            {
                if (_LIST_PEERS != value)
                {
                    _LIST_PEERS = value;
                    LIST_PEERS.SelectionChanged += LIST_PEERS_SelectionChanged;
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

        #endregion

        #region Constructors

        public ViewModelMainPage()
        {
            isConnectionPossible = false;

            _refreshDevicesCommand = new DelegateCommand(ExecuteRefreshDevicesCommand);
            _connectToDeviceCommand = new DelegateCommand(ExecuteConnectToDeviceCommand);

            _onNavigateTo = new DelegateCommand(OnNavigatedTo);
            _onNavigateFrom = new DelegateCommand(OnNavigatingFrom);
        }

        #endregion

        #region Methods

        private void ExecuteRefreshDevicesCommand(object parameter)
        {
            RefreshPeerAppList();
        }

        private void ExecuteConnectToDeviceCommand(object parameter)
        {
            if (LIST_PEERS.SelectedItem == null)
            {
                MessageBox.Show(AppResources.Err_NoPeer, AppResources.Err_NoConnectTitle, MessageBoxButton.OK);
                return;
            }

            // Connect to the selected peer.
            PeerAppInfo pdi = LIST_PEERS.SelectedItem as PeerAppInfo;
            PeerInformation peer = pdi.PeerInfo;

            ConnectToPeer(peer);
        }


        public void OnNavigatedTo(object parameter)
        {
            // Maintain a list of peers and bind that list to the UI
            _peerApps = new ObservableCollection<PeerAppInfo>();
            LIST_PEERS.ItemsSource = _peerApps;

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

                    // If there is only one peer, go ahead and select it
                    if (LIST_PEERS.Items.Count == 1)
                        LIST_PEERS.SelectedIndex = 0;

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
