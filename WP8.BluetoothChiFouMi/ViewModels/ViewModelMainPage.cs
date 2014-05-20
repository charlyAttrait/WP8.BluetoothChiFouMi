using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;
using WP8.BluetoothChiFouMi.Resources;
using WP8.Core;

namespace WP8.BluetoothChiFouMi.ViewModels
{
    public class ViewModelMainPage : ObservableObject
    {
        #region Fields

        private DataWriter _dataWriter;
        private DataReader _dataReader;

        private DelegateCommand _refreshDevicesCommand;
        private DelegateCommand _connectToDeviceCommand;
        private DelegateCommand _onNavigateTo;
        private DelegateCommand _onNavigateFrom;
        private DelegateCommand _selectionPeerChanged;

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
        private int _SelectedPivotItem;

        private string _OpponentPseudo;

        private Visibility _isGameVisible;
        private Visibility _isTimerVisible;

        private Score _CurrentScore;

        private DelegateCommand _ChoiceCommand; // Command du choix de signe (Pierre, Papier, Ciseaux)
        private DelegateCommand _StartTimer; // Command de démarrage du Timer
        private DelegateCommand _ResetTimer; // Command de réinitialisation du Timer

        private string _MyChoice; // Récupération du choix de l'utilisateur
        private string _OpponentChoice; // Récupération du choix de l'adversaire

        private DispatcherTimer _DispatchTimer;
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
        public Visibility isTimerVisible
        {
            get { return _isTimerVisible; }
            set
            {
                if (_isTimerVisible != value)
                {
                    _isTimerVisible = value;
                    onPropertyChanged();
                }
            }
        }

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

        public DelegateCommand selectionPeerChanged
        {
            get { return _selectionPeerChanged; }
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
        public int SelectedPivotItem
        {
            get { return _SelectedPivotItem; }
            set 
            {
                if (_SelectedPivotItem != value)
                {
                    _SelectedPivotItem = value;
                    onPropertyChanged();
                }
            }
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
        public string OpponentChoice
        {
            get { return _OpponentChoice; }
            set
            {
                if (_OpponentChoice != value)
                {
                    _OpponentChoice = value;
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
            isTimerVisible = Visibility.Collapsed;
        }

        #endregion

        #region BLUETOOTH METHODS

        /// <summary>
        /// Commande pour raffraichir la liste des appareils à proximité
        /// </summary>
        /// <param name="parameter"></param>
        private void ExecuteRefreshDevicesCommand(object parameter)
        {
            RefreshPeerAppList();
        }
        /// <summary>
        /// Commande de connexion à l'appareil sélectionné
        /// </summary>
        /// <param name="parameter"></param>
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

            if (UserPseudo != "")
            {
                RefreshPeerAppList();
            }
        }
        public void OnNavigatingFrom(object parameter)
        {
            PeerFinder.ConnectionRequested -= PeerFinder_ConnectionRequested;

            // Cleanup before we leave
            CloseConnection(false);
        }


        /// <summary>
        /// Méthode appelé lorsque l'appareil à été sélectionné par un autre utilisateur
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void PeerFinder_ConnectionRequested(object sender, ConnectionRequestedEventArgs args)
        {
            try
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
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
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                CloseConnection(true);
            }
        }
        /// <summary>
        /// Méthode de connexion à l'appareil sélectionné dans la liste
        /// </summary>
        /// <param name="peer"></param>
        async void ConnectToPeer(PeerInformation peer)
        {
            try
            {
                _socket = await PeerFinder.ConnectAsync(peer);

                // We can preserve battery by not advertising our presence.
                PeerFinder.Stop();

                _peerName = peer.DisplayName;

                ListenForTimerState();

                OpponentPseudo = peer.DisplayName;

                isGameVisible = Visibility.Visible;
                SelectedPivotItem += 1;
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

        /// <summary>
        /// Méthode asynchrone : attente du choix de l'adversaire
        /// </summary>
        private async void ListenForOpponentChoice()
        {
            try
            {
                var choice = await GetChoice();
                if (choice != "" || choice != null)
                {
                    OpponentChoice = choice;
                }

                // Start listening for the opponent choice
                ListenForOpponentChoice();
            }
            catch (Exception)
            {
                CloseConnection(true);
            }
        }
        private async void ListenForTimerState()
        {
            try
            {
                var state = await GetTimerState();
                if (state == "true")
                {
                    ExecuteStartTimer("");
                }
                    ListenForTimerState();
            }
            catch (Exception)
            {
                CloseConnection(true);
            }
        }

        /// <summary>
        /// Réception du choix de l'adversaire (afin de l'afficher)
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetChoice()
        {
            if (_dataReader == null)
                _dataReader = new DataReader(_socket.InputStream);

            // The first is the size of the choice.
            await _dataReader.LoadAsync(4);
            uint messageLen = (uint)_dataReader.ReadInt32();

            // The second if the choice itself.
            await _dataReader.LoadAsync(messageLen);
            return _dataReader.ReadString(messageLen);
        }
        private async Task<string> GetTimerState()
        {
            if (_dataReader == null)
                _dataReader = new DataReader(_socket.InputStream);

            // The first is the size of the timerState.
            await _dataReader.LoadAsync(4);
            uint messageLen = (uint)_dataReader.ReadInt32();

            // The second if the timerState itself.
            await _dataReader.LoadAsync(messageLen);
            return _dataReader.ReadString(messageLen);
        }
        /// <summary>
        /// Envoi du choix utilisateur à l'adversaire (afin de l'afficher)
        /// </summary>
        /// <param name="choice"></param>
        private async void SendChoice(string choice)
        {
            if (choice != null)
            {
                if (_dataWriter == null)
                    _dataWriter = new DataWriter(_socket.OutputStream);

                // The first is the size of the choice.
                _dataWriter.WriteInt32(choice.Length);
                await _dataWriter.StoreAsync();

                // The second if the choice itself.
                _dataWriter.WriteString(choice);
                await _dataWriter.StoreAsync();
            }
        }
        private async void SetTimerState()
        {
            if (_dataWriter == null)
                _dataWriter = new DataWriter(_socket.OutputStream);

            // The first is the size of the timerState.
            _dataWriter.WriteInt32("true".Length);
            await _dataWriter.StoreAsync();

            // The second if the timerState itself.
            _dataWriter.WriteString("true");
            await _dataWriter.StoreAsync();
        }

        /// <summary>
        /// Fermeture de la connexion avec l'appareil
        /// </summary>
        /// <param name="continueAdvertise"></param>
        private void CloseConnection(bool continueAdvertise)
        {
            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
                isGameVisible = Visibility.Collapsed;
                SelectedPivotItem -= 1;
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
        /// Raffraichissement de la liste des appareils à proximité (appareil ayant lancé l'application)
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

        /// <summary>
        /// Fonction manipulant la barre de progression lors de la recherche d'appareils
        /// </summary>
        /// <param name="message"></param>
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
        /// Ouvre la page paramètre Bluetooth (afin d'activer les connexions Bluetooth)
        /// </summary>
        private void ShowBluetoothControlPanel()
        {
            ConnectionSettingsTask connectionSettingsTask = new ConnectionSettingsTask();
            connectionSettingsTask.ConnectionSettingsType = ConnectionSettingsType.Bluetooth;
            connectionSettingsTask.Show();
        }

        #endregion

        /// <summary>
        /// Action des boutons images
        /// </summary>
        /// <param name="parameter">Signe Chi/Fou/Mi choisi par l'utilisateur</param>
        public void ExecuteChoiceCommand(object parameter)
        {
            if (CountDown == 0 && _DispatchTimer != null && MyChoice == null)
            {
                MyChoice = "/Assets/SIGLES/" + parameter.ToString() + ".png";
            }
        }

        #region TIMER

        /// <summary>
        /// Démarre le Timer du Jeu
        /// </summary>
        /// <param name="parameter"></param>
        public void ExecuteStartTimer(object parameter)
        {
            if (parameter == null)
            {
                SetTimerState();
            }
            
            CountDown = 3;
            _DispatchTimer = new DispatcherTimer();
            _DispatchTimer.Interval = TimeSpan.FromMilliseconds(1000);
            _DispatchTimer.Tick += _DispatchTimer_Tick;
            _DispatchTimer.Start();

            isTimerEnabled = false;
            isResetTimerEnabled = false;
            isTimerVisible = Visibility.Visible;
        }

        /// <summary>
        /// Décompte du Timer
        /// </summary>
        void _DispatchTimer_Tick(object sender, EventArgs e)
        {
                if (CountDown > 0)
                {
                    CountDown--;
                }
                else
                {
                    isTimerVisible = Visibility.Collapsed;
                    isResetTimerEnabled = true;
                    _DispatchTimer.Stop();
                    _DispatchTimer = null;
                    SendChoice(MyChoice);
                    ListenForOpponentChoice();
                }
        }

        /// <summary>
        /// Reinitialise le Timer du Jeu
        /// </summary>
        /// <param name="parameter"></param>
        public void ExecuteResetTimer(object parameter)
        {
            // Si le Timer est terminé, on peut le réinitialiser
            if (isResetTimerEnabled)
            {
                CountDown = 3;
                isTimerEnabled = true;
                MyChoice = null;
                OpponentChoice = null;
            }
        }

        #endregion

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
