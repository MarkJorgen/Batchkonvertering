using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace dk.gi.app.konto.aarligrentetilskrvning
{

    [DataContract]
    public class KontoInterval
    {
        [DataMember(Name = "KontoFra")]
        public string KontoFra { get; set; }

        [DataMember(Name = "KontoTil")]
        public string KontoTil { get; set; }

        public KontoInterval() { }
    }
}
