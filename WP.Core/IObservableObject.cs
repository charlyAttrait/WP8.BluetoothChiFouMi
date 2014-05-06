using System;
namespace WP8.Core
{
    interface IObservableObject
    {
        event global::System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        event global::System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
    }
}
