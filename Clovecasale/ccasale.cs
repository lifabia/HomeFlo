using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


[Serializable]
namespace Clovecasale
{
    public class ccasale
    {

    }
     [Serializable]
    public class cUte : clogin
    {
        public string nome { get; set; }
        public int iden { get; set; }
        public List<cZone> zone;

    }
    public class cZone
    {
        public string nomeZona { get; set; }
        private int idzona { get; set; }
       
        public List<cImg> immagini;
        public List<cZone> dettagliZona;
    }
    public class cImg
    {
        public string nomeImg { get; set; }
        private int idimg { get; set; }
        public string urlImg { get; set; }

        public byte[] img { get; set; }

    }
}
