﻿namespace Milochau.Emails.Sdk.Models
{
    /// <summary>Emails service settings</summary>
    public class EmailsServiceSettings
    {
        /// <summary>Namespace of the Service bus used by the Emails microservice to send emails</summary>
        /// <remarks>Should be formatted as: xxx.servicebus.windows.net</remarks>
        public string ServiceBusNamespace { get; set; }

        /// <summary>Name of the Storage Account used by the Emails microservice store emails attachments</summary>
        /// <remarks>Should be formatted as: </remarks>
        public string StorageAccountName { get; set; }
    }
}
