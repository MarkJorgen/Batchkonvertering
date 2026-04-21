namespace dk.gi.app.konto.satser.slet.Application.Models
{
    public sealed class SletSatserSettings
    {
        public JobExecutionMode Mode { get; set; }

        public string AuthorityMode { get; set; }

        public string RuntimeEngine { get; set; }

        public bool EnableLegacySettingWriteOut { get; set; }

        public int SatsAar { get; set; }

        public int TimeOutMinutter { get; set; }

        public int SecondsToSleep { get; set; }

        public int MaxWaitCount { get; set; }

        public string CrmConnectionTemplate { get; set; }

        public string CrmServerName { get; set; }

        public string CrmClientId { get; set; }

        public string CrmClientSecret { get; set; }

        public string CrmAuthority { get; set; }

        public string CrmOrganisationName { get; set; }

        public string CrmUserName { get; set; }

        public string CrmUserPassword { get; set; }

        public string FailureRecipients { get; set; }
    }
}
