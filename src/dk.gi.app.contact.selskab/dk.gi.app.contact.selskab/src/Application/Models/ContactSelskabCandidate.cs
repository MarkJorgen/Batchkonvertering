using System;

namespace dk.gi.app.contact.selskab.Application.Models
{
    public sealed class ContactSelskabCandidate
    {
        public Guid CompanyId { get; }
        public string CvrNumber { get; }
        public int OwnersWithKdkYes { get; }
        public int OwnersWithKdkNo { get; }

        public ContactSelskabCandidate(Guid companyId, string cvrNumber, int ownersWithKdkYes, int ownersWithKdkNo)
        {
            CompanyId = companyId;
            CvrNumber = cvrNumber ?? string.Empty;
            OwnersWithKdkYes = ownersWithKdkYes;
            OwnersWithKdkNo = ownersWithKdkNo;
        }
    }
}
