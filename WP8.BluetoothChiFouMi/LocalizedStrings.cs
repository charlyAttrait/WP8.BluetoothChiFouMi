using WP8.BluetoothChiFouMi.Resources;

namespace WP8.BluetoothChiFouMi
{
    /// <summary>
    /// Permet d'accéder aux ressources de chaîne.
    /// </summary>
    public class LocalizedStrings
    {
        private static AppResources _localizedResources = new AppResources();

        public AppResources LocalizedResources { get { return _localizedResources; } }
    }
}