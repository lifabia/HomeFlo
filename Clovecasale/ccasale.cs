using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clovecasale
{
    [Serializable]
    public class cUte : clogin
    {
        private int _idutente;
        public int Idutente {
        get { return _idutente; }
            set { _idutente = value; }
        }
        public string nome { get; set; }
       
        private List<cZone> _zone = new List<cZone>();
        public List<cZone> Zone
        {
            get { return _zone; }
            set { _zone = value; }
        }

    }

    [Serializable]
    public class cZone
    {
        public cZone(int idutente)
        {

        }
        public string nomeZona { get; set; }
        private int idzona { get; set; }
       
        public List<cImg> immagini;
        public List<cZone> dettagliZona;
    }

    [Serializable]
    public class cImg : cGalleria
    {
        public string nomeImg { get; set; }
        private int idimg { get; set; }
        public string urlImg { get; set; }

        public byte[] img { get; set; }

    }

    [Serializable]
    public class cGalleria
    {
        public int id { get; set; }
        public string titolo { get; set; }
    }
}
