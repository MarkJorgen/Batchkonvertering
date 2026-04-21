using System;

namespace dk.gi.app.contact.selskab.Application.Models
{
    public sealed class ContactSelskabOwnerObservation
    {
        public Guid CompanyId { get; }
        public string CvrNumber { get; }
        public bool UltimateOwnerHasKdk { get; }

        public ContactSelskabOwnerObservation(Guid companyId, string cvrNumber, bool ultimateOwnerHasKdk)
        {
            CompanyId = companyId;
            CvrNumber = cvrNumber ?? string.Empty;
            UltimateOwnerHasKdk = ultimateOwnerHasKdk;
        }
    }
}
