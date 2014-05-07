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

        private DelegateCommand _ChoiceCommand; // Command du choix de signe (Pierre, Papier, Ciseaux)
        private DelegateCommand _StartTimer; // Command de démarrage du Timer
        private DelegateCommand _ResetTimer; // Command de réinitialisation du Timer

        private string _MyChoice; // Récupération du choix de l'utilisateur

        private DispatcherTimer _Timer;
        private int tik; // Valeur de départ du décompte du Timer
        private string _CountDown; // Valeur à afficher
        private Boolean _isTimerEnabled; // Booleen pour vérrouiller le bouton de lancement du Timer
        private Boolean _isResetTimerEnabled; // Boolean pour vérouiller le bouton de réinitialisation du Timer

        private ObservableCollection<string> _Records; // Liste des scores

        #endregion

        #region Properties

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

        public string CountDown
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

        public ObservableCollection<string> Records
        {
            get { return _Records; }
            set { _Records = value; }
        }

        #endregion

        #region Constructors

        public ViewModelMainPage()
        {
            _ChoiceCommand = new DelegateCommand(ExecuteChoiceCommand);

            _StartTimer = new DelegateCommand(ExecuteStartTimer);
            _ResetTimer = new DelegateCommand(ExecuteResetTimer);

            _Records = new ObservableCollection<string>();

            isTimerEnabled = true;
            CountDown = "3";
        }

        #endregion

        #region Methods

        public void ExecuteChoiceCommand(object parameter)
        {
            if (_Timer != null)
            {
                MyChoice = "/Assets/SIGLES/Left_" + parameter.ToString() + ".png";

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
        }

        public void ExecuteStartTimer(object parameter)
        {
            try
            {
                _Timer = new DispatcherTimer();
                _Timer.Interval = new TimeSpan(0, 0, 1);
                _Timer.Tick += new EventHandler(timer1_Tick);
                tik = 3;
                _Timer.Start();
                isTimerEnabled = false;
                isResetTimerEnabled = false;
            }
            catch (Exception ex)
            {
            }
        }
        void timer1_Tick(object sender, EventArgs e)
        {
            CountDown = tik.ToString();
            if (tik > 0)
            {
                tik--;
            }
            else
            {
                CountDown = "Times Up";
                isResetTimerEnabled = true;
                _Timer.Stop();
                _Timer = null;
            }
        }

        public void ExecuteResetTimer(object parameter)
        {
            // Si le Timer est terminé, on peut le réinitialiser
            if (isResetTimerEnabled)
            {
                tik = 3;
                CountDown = tik.ToString();
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
                    // Reading Records File
                    using (StreamReader streamReader = new StreamReader(file.Path))
                    {
                        Records.Add(streamReader.ReadLine());
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
}
