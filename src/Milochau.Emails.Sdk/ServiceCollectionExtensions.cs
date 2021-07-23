using Milochau.Emails.Sdk.DataAccess;
using Milochau.Emails.Sdk.Models;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using System;
using Milochau.Emails.Sdk.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.ServiceBus.Primitives;

namespace Milochau.Emails.Sdk
{
    /// <summary>Extensions for <see cref="IServiceCollection"/></summary>
    public static class ServiceCollectionExtensions
    {
        private const string serviceBusEndpointName = "Emails microservice (Service Bus)";
        private const string serviceBusQueueNameEmails = "emails";

        /// <summary>Register emails clients, to be accessed from dependency injection</summary>
        /// <param name="services">Service collection</param>
        /// <param name="settings">Settings</param>
        public static IServiceCollection AddEmailsClients(this IServiceCollection services, Action<EmailsServiceSettings> settings)
        {
            var settingsValue = new EmailsServiceSettings();
            settings.Invoke(settingsValue);

            // Add helpers
            services.AddSingleton<IEmailsValidationHelper, EmailsValidationHelper>();

            // Add services for ServiceBus communication
            if (!string.IsNullOrEmpty(settingsValue.ServiceBusEndpoint) && !string.IsNullOrEmpty(serviceBusQueueNameEmails))
            {
                var tokenProvider = TokenProvider.CreateManagedIdentityTokenProvider();

                services.AddSingleton<IEmailsClient>(serviceProvider =>
                {
                    var queueClient = new QueueClient(settingsValue.ServiceBusEndpoint, serviceBusQueueNameEmails, tokenProvider);
                    var emailsValidationHelper = serviceProvider.GetRequiredService<IEmailsValidationHelper>();
                    var logger = serviceProvider.GetRequiredService<ILogger<EmailsServiceBusClient>>();
                    return new EmailsServiceBusClient(queueClient, emailsValidationHelper, logger);
                });
            }

            return services;
        }
    }
}
