using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clovecasale
{
   
    public static class clogin
    {
        public string utente { get; set; }
        public string login { get; set; }
        private string pwd;
        private List<keyValue> _preferiti;
        public List<keyValue> Preferiti
        {
            get { return _preferiti; }
            set { _preferiti = value; }
        }

    }
    public class keyValue : IEnumerable 
    {
        public string chiave { get; set; }
        public string valore { get; set; }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
