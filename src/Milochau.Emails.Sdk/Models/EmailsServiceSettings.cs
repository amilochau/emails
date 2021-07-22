namespace Milochau.Emails.Sdk.Models
{
    /// <summary>Emails service settings</summary>
    public class EmailsServiceSettings
    {
        /// <summary>Endpoint to the Service bus used by the Emails microservice</summary>
        /// <remarks>Should be formatted as: sb://xxx.servicebus.windows.net/</remarks>
        public string ServiceBusEndpoint { get; set; }
    }
}
