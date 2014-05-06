using System;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Runtime.CompilerServices;

namespace WP8.Core
{
    /// <summary>
    /// Classe de base pour les objets source de binding, implémente INotifyPropertyChanged & INotifyPropertyChanging
    /// </summary>
    public class ObservableObject: INotifyPropertyChanged, INotifyPropertyChanging, WP8.Core.IObservableObject
    {
        /// <summary>
        /// Se déclenche avant la modification d'une valeur de l'objet
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging;
        /// <summary>
        /// Se déclenche lorsqu'une propriété de l'objet a changé de valeur
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Permet de déclencher l'évenement PropertyChanging
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void onPropertyChanging([CallerMemberName] string propertyName = "")
        {
            // Créer une copie de l'évenement PropertyChanging (évite les passages à null par un autre thread)
            PropertyChangingEventHandler handler = PropertyChanging;

            if (handler != null)
            {
                handler(this, new PropertyChangingEventArgs(propertyName));
            }
        }
        /// <summary>
        /// Permet de déclencher l'évenement PropertyChanged
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void onPropertyChanged([CallerMemberName] string propertyName = "")
        {
            // Créer une copie de l'évenement PropertyChanged (évite les passages à null par un autre thread)
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
