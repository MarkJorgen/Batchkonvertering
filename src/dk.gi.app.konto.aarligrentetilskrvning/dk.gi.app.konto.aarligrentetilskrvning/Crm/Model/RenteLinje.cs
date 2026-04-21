using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dk.gi.app.konto.aarligrentetilskrvning
{
    public class RenteLinje
    {
        public string TransDate { get; set; }

        public string ValueDate { get; set; }

        public string RelationYear { get; set; }

        public string AccountNum { get; set; }
        public string AccountType { get; set; }

                //if (AccountTypeModtaget == 0)
                //{
                //    return "Ledger";
                //}

                //if (AccountTypeModtaget == 2)
                //{
                //    return "Vend";
                //}

        public string Amount { get; set; }

        //public string Txt { get; set; }

        public string Dimension_Department { get; set; }

        public string Dimension_Project { get; set; }

    }
}
