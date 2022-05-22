using Milochau.Emails.Sdk.DataAccess;
using Milochau.Emails.Sdk.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using Milochau.Emails.Sdk.Helpers;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;

namespace Milochau.Emails.Sdk
{
    /// <summary>Extensions for <see cref="IServiceCollection"/></summary>
    public static class ServiceCollectionExtensions
    {
        private const string serviceBusQueueNameEmails = "emails";
        private const string azureStorageContainerName = "attachments";

        /// <summary>Register emails clients, to be accessed from dependency injection</summary>
        /// <param name="services">Service collection</param>
        /// <param name="settings">Settings</param>
        public static IServiceCollection AddEmailsClients(this IServiceCollection services, Action<EmailsServiceSettings> settings)
        {
            var settingsValue = new EmailsServiceSettings();
            settings.Invoke(settingsValue);

            // Add helpers
            services.AddSingleton<IEmailsValidationHelper, EmailsValidationHelper>();

            // Add services for Azure Storage Account
            services.AddSingleton<IAttachmentsClient>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<AttachmentsStorageClient>>();

                var credentialOptions = GetCredentialOptions(settingsValue);
                var credential = new DefaultAzureCredential(credentialOptions);
                var blobServiceClient = new BlobServiceClient(settingsValue.StorageAccountUri, credential);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(azureStorageContainerName);
                return new AttachmentsStorageClient(blobContainerClient, logger);
            });

            // Add services for Azure Service Bus
            services.AddSingleton<IEmailsClient>(serviceProvider =>
            {
                var emailsValidationHelper = serviceProvider.GetRequiredService<IEmailsValidationHelper>();
                var logger = serviceProvider.GetRequiredService<ILogger<EmailsServiceBusClient>>();

                var credentialOptions = GetCredentialOptions(settingsValue);
                var credential = new DefaultAzureCredential(credentialOptions);
                var serviceBusClient = new ServiceBusClient(settingsValue.ServiceBusNamespace, credential);
                var serviceBusSender = serviceBusClient.CreateSender(serviceBusQueueNameEmails);

                return new EmailsServiceBusClient(serviceBusSender, emailsValidationHelper, logger);
            });

            return services;
        }

        private static DefaultAzureCredentialOptions GetCredentialOptions(EmailsServiceSettings settingsValue)
        {
            var credentialOptions = new DefaultAzureCredentialOptions();

            if (!string.IsNullOrWhiteSpace(settingsValue.ManagedIdentityClientId))
            {
                credentialOptions.ManagedIdentityClientId = settingsValue.ManagedIdentityClientId;
            }

            return credentialOptions;
        }
    }
}
