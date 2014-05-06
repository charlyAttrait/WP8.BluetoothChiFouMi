using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WP8.Core
{
    public class DelegateCommand : ICommand
    {
        #region Events

        public event EventHandler CanExecuteChanged;

        #endregion

        #region Field

        /// <summary>
        ///     On créer un délégué, c'est à dire un conteneur de méthode.
        ///     La méthode prend un paramètre une variable de type objet.
        /// </summary>
        private Action<object> _ExecuteDelegate;

        private Func<object, bool> _CanExecuteDelegate;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initialise une nouvelle instance de la classe WP8.Core.DelegateCommand.
        /// </summary>
        /// <param name="executedDelegate">Méthode à exécuter pour la commande.</param>
        /// <param name="canExecuteDelegate">Méthode à exécuter pour le test de la commande.</param>
        public DelegateCommand(Action<object> executeDelegate, Func<object,bool> canExecuteDelegate = null)
        {
            //On stock dans le délégué la méthode à appeler.
            _ExecuteDelegate = executeDelegate;
            _CanExecuteDelegate = canExecuteDelegate;
        }

        #endregion

        #region Methods

        public void OnCanExecuteChanged()
        {
            EventHandler tempHandler = CanExecuteChanged;

            if (tempHandler != null)
            {
                tempHandler(this, new EventArgs());
            }
        }

        /// <summary>
        ///     Vérifie si la commande peut être exécutée.
        /// </summary>
        /// <param name="parameter">Paramètre de la commande</param>
        /// <returns>Détermine si la commande peut être exécutée.</returns>
        public bool CanExecute(object parameter)
        {
            bool result = true;

            if (_CanExecuteDelegate != null)
            {
                result = _CanExecuteDelegate(parameter);
            }

            return result;
        }

        /// <summary>
        ///     Exécute la commande.
        /// </summary>
        /// <param name="parameter">Paramètre de la commande</param>
        public void Execute(object parameter)
        {
            if(_ExecuteDelegate != null && CanExecute(parameter)) //Si on a bien une méthode
            {
                //On appel la méthode
                _ExecuteDelegate(parameter);
            }
        }

        #endregion


    }
}
