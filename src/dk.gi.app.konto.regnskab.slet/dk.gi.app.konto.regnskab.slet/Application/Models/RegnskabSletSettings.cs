namespace dk.gi.app.konto.regnskab.slet.Application.Models
{
    public sealed class RegnskabSletSettings
    {
        public JobExecutionMode Mode { get; set; }
        public string AuthorityMode { get; set; }
        public string RuntimeEngine { get; set; }
        public string CrmConnectionTemplate { get; set; }
        public string CrmServerName { get; set; }
        public string CrmClientId { get; set; }
        public string CrmClientSecret { get; set; }
        public string CrmAuthority { get; set; }
        public string ServiceBusBaseUrl { get; set; }
        public string ServiceBusSasKeyName { get; set; }
        public string ServiceBusSasKey { get; set; }
        public string ServiceBusQueueName { get; set; }
        public string ServiceBusLabel { get; set; }
        public string ServiceBusSessionId { get; set; }
        public int DelayStepSeconds { get; set; }
        public int DefaultBatchCount { get; set; }
        public string FailureRecipients { get; set; }
    }
}
