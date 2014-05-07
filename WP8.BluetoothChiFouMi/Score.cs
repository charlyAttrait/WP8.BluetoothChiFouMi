using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WP8.BluetoothChiFouMi
{
    public class Score
    {
        #region Fields

        private string _Joueur1;
        private int _VictoiresJoueur1;
        private int _VictoiresJoueur2;
        private string _Joueur2;

        #endregion

        #region Properties

        public string Joueur1
        {
            get { return _Joueur1; }
            set { _Joueur1 = value; }
        }

        public int VictoiresJoueur1
        {
            get { return _VictoiresJoueur1; }
            set { _VictoiresJoueur1 = value; }
        }

        public int VictoiresJoueur2
        {
            get { return _VictoiresJoueur2; }
            set { _VictoiresJoueur2 = value; }
        }

        public string Joueur2
        {
            get { return _Joueur2; }
            set { _Joueur2 = value; }
        } 

        #endregion

        #region Constructors

        public Score(string joueur1, int victoiresJ1, int victoiresJ2, string joueur2)
        {
            this._Joueur1 = joueur1;
            this._VictoiresJoueur1 = victoiresJ1;
            this._VictoiresJoueur2 = victoiresJ2;
            this._Joueur2 = joueur2;
        }

        #endregion
    }
}
