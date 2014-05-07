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

        #region Bluetooth Fields

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

        private string _TXT_PEER_Text;
        private object _LIST_SelectedItem;
        private IEnumerable _LIST_ItemsSource;
        private Boolean _isConnectionPossible;

        #endregion

        private string _OpponentPseudo;

        private Visibility _isGameVisible;

        private Score _CurrentScore;

        private DelegateCommand _ChoiceCommand; // Command du choix de signe (Pierre, Papier, Ciseaux)
        private DelegateCommand _StartTimer; // Command de démarrage du Timer
        private DelegateCommand _ResetTimer; // Command de réinitialisation du Timer

        private string _MyChoice; // Récupération du choix de l'utilisateur

        //private DispatcherTimer _Timer;
        private System.Threading.Timer _Timer;
        private int tik; // Valeur de départ du décompte du Timer
        private int _CountDown; // Valeur à afficher
        private Boolean _isTimerEnabled; // Booleen pour vérrouiller le bouton de lancement du Timer
        private Boolean _isResetTimerEnabled; // Boolean pour vérouiller le bouton de réinitialisation du Timer

        private ObservableCollection<Score> _Records; // Liste des scores

        #endregion

        #region Properties

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
        public string OpponentPseudo
        {
            get 
            {
                if (_OpponentPseudo == null)
                {
                    OpponentPseudo = "Florian";
                }
                return _OpponentPseudo; 
            }
            set
            {
                if (value != _OpponentPseudo)
                {
                    _OpponentPseudo = value;
                    onPropertyChanged();
                }
            }
        }

        public Visibility isGameVisible
        {
            get { return _isGameVisible; }
            set
            {
                if (_isGameVisible != value)
                {
                    _isGameVisible = value;
                    onPropertyChanged();
                }
            }
        }

        #region Bluetooth Properties

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

        #endregion

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

        public int CountDown
        {
            get { return _CountDown; }
            set
            {
                if (_CountDown != value)
                {
                    _CountDown = value;
                    onPropertyChanged();
                }
            }
        }
        public Boolean isTimerEnabled
        {
            get { return _isTimerEnabled; }
            set
            {
                if (_isTimerEnabled != value)
                {
                    _isTimerEnabled = value;
                    onPropertyChanged();
                }
            }
        }
        public Boolean isResetTimerEnabled
        {
            get { return _isResetTimerEnabled; }
            set
            {
                if (_isResetTimerEnabled != value)
                {
                    _isResetTimerEnabled = value;
                    onPropertyChanged();
                }
            }
        }
        public DelegateCommand StartTimer
        {
            get { return _StartTimer; }
            set { _StartTimer = value; }
        }
        public DelegateCommand ResetTimer
        {
            get { return _ResetTimer; }
            set { _ResetTimer = value; }
        }

        public ObservableCollection<Score> Records
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
            _ResetTimer = new DelegateCommand(ExecuteResetTimer);

            _Records = new ObservableCollection<Score>();

            isTimerEnabled = true;
            CountDown = 3;
            isGameVisible = Visibility.Collapsed;
        }

        #endregion

        #region Methods

        #region Bluetooth Methods

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

                isGameVisible = Visibility.Visible;
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

        #endregion

        public void ExecuteChoiceCommand(object parameter)
        {
            if (tik == 0 && _Timer != null && MyChoice == null)
            {
                MyChoice = "/Assets/SIGLES/Left_" + parameter.ToString() + ".png";
            }

            switch (parameter.ToString())
            {
                case "Chi":

                    break;
                case "Fou":

                    break;
                case "Mi":

                    break;
                default:
                    break;
            }
        }

        public void ExecuteStartTimer(object parameter)
        {
            tik = 3;
            _Timer = new System.Threading.Timer(new System.Threading.TimerCallback(Timer_Tick), null, 1000, 1000);
            isTimerEnabled = false;
            isResetTimerEnabled = false;
        }

        private void Timer_Tick(object state)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (tik > 0)
                {
                    tik--;
                }
                else
                {
                    isResetTimerEnabled = true;
                    _Timer.Dispose();
                    _Timer = null;
                }
                CountDown = tik;
            });
        }

        public void ExecuteResetTimer(object parameter)
        {
            // Si le Timer est terminé, on peut le réinitialiser
            if (isResetTimerEnabled)
            {
                tik = 3;
                CountDown = tik;
                isTimerEnabled = true;
                MyChoice = null;
            }
        }

        /// <summary>
        /// Remplissage du tableau des scores
        /// </summary>
        /// <param name="record"></param>
        private async void fillRecordsTab(string record = "")
        {
            // Dossier du fichier des scores
            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;

            if (local != null)
            {
                // OUVRIR / CREER le dossier stockant le fichier des scores
                var dataFolder = await local.CreateFolderAsync("DataFolder", CreationCollisionOption.OpenIfExists);

                // OUVRIR / CREER le fichier des scores
                var file = await dataFolder.CreateFileAsync("Records.txt", CreationCollisionOption.OpenIfExists);

                if (record == "") // LECTURE DES SCORES
                {
                    string[] tab = null;
                    // Reading Records File
                    using (StreamReader streamReader = new StreamReader(file.Path))
                    {
                        tab = streamReader.ReadLine().Split(';');
                        Records.Add(new Score(tab[0], int.Parse(tab[1]), int.Parse(tab[2]), tab[3]));
                    }
                }
                else // ECRITUTE D'UN SCORE
                {
                    // Ecrire le record sous forme d'un tableau de byte
                    byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(record);

                    // Ecriture dans le fichier
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
