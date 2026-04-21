using System;

namespace dk.gi.app.konto.afslutarealsager.Application.Models
{
    public sealed class KontoAfslutArealSagerCandidate
    {
        public Guid CaseId { get; }
        public string CaseNumber { get; }
        public DateTime CreatedOn { get; }
        public Guid AccountId { get; }
        public string AccountNumber { get; }
        public Guid ContactId { get; }
        public string ContactName { get; }
        public string GovernmentId { get; }
        public string CompanyId { get; }
        public string AddressLine1 { get; }
        public string PostalCode { get; }
        public string City { get; }
        public Guid PropertyId { get; }
        public string PropertyAddress { get; }
        public DateTime? LastAccountingDate { get; }

        public KontoAfslutArealSagerCandidate(
            Guid caseId,
            string caseNumber,
            DateTime createdOn,
            Guid accountId,
            string accountNumber,
            Guid contactId,
            string contactName,
            string governmentId,
            string companyId,
            string addressLine1,
            string postalCode,
            string city,
            Guid propertyId,
            string propertyAddress,
            DateTime? lastAccountingDate)
        {
            CaseId = caseId;
            CaseNumber = caseNumber ?? string.Empty;
            CreatedOn = createdOn;
            AccountId = accountId;
            AccountNumber = accountNumber ?? string.Empty;
            ContactId = contactId;
            ContactName = contactName ?? string.Empty;
            GovernmentId = governmentId ?? string.Empty;
            CompanyId = companyId ?? string.Empty;
            AddressLine1 = addressLine1 ?? string.Empty;
            PostalCode = postalCode ?? string.Empty;
            City = city ?? string.Empty;
            PropertyId = propertyId;
            PropertyAddress = propertyAddress ?? string.Empty;
            LastAccountingDate = lastAccountingDate;
        }

        public bool HasRecipientIdentifier => string.IsNullOrWhiteSpace(GovernmentId) == false || string.IsNullOrWhiteSpace(CompanyId) == false;
        public bool HasAddress => string.IsNullOrWhiteSpace(AddressLine1) == false && string.IsNullOrWhiteSpace(PostalCode) == false && string.IsNullOrWhiteSpace(City) == false;
        public bool IsUsableForLetter => HasRecipientIdentifier && HasAddress && string.IsNullOrWhiteSpace(PropertyAddress) == false;
    }
}
