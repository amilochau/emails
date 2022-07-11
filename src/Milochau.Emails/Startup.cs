using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Milochau.Core.Functions;
using Milochau.Emails.Sdk.Helpers;
using Milochau.Emails.DataAccess;
using Milochau.Emails.Options;
using Milochau.Emails.Services;
using Milochau.Emails.Services.EmailTemplates;
using SendGrid;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.Storage.Blobs;
using System;
using Milochau.Core.Abstractions;
using Milochau.Core.Cosmos;
using Milochau.Emails.DataAccess.Implementations;
using Milochau.Emails.Services.Implementations;

namespace Milochau.Emails
{
    public class Startup : CoreFunctionsStartup
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            RegisterOptions(services);
            RegisterServices(services);
            RegisterDataAccess(services);
        }

        private void RegisterOptions(IServiceCollection services)
        {
            services.Configure<EmailsOptions>(options => Configuration.GetSection("Emails").Bind(options))
                .PostConfigure<EmailsOptions>(options =>
                {
                    options.StorageAccountUri ??= HostOptions.Application.GetInfrastructureConvention(InfrastructureConventionType.StorageAccountUri, "1");
                });
            services.Configure<SendGridOptions>(options => Configuration.GetSection("SendGrid").Bind(options));
        }

        private void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IEmailsService, EmailsService>();
            services.AddScoped<ITranslationService, TranslationService>();
            services.AddScoped<IEmailTemplateFactory, EmailTemplateFactory>();

            services.AddScoped<IEmailsValidationHelper, EmailsValidationHelper>();
        }

        private void RegisterDataAccess(IServiceCollection services)
        {
            services.AddCosmosDb();

            services.AddSingleton<IEmailsDataAccess, EmailsSendGridClient>();

            services.AddSingleton<IStorageDataAccess>(serviceProvider =>
            {
                var emailsOptions = serviceProvider.GetRequiredService<IOptions<EmailsOptions>>().Value;
                var logger = serviceProvider.GetRequiredService<ILogger<StorageDataAccess>>();

                var credential = new DefaultAzureCredential(HostOptions.Credential);
                var blobServiceClient = new BlobServiceClient(new Uri(emailsOptions.StorageAccountUri), credential);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(StorageDataAccess.DefaultContainerName);
                return new StorageDataAccess(blobContainerClient, logger);
            });

            services.AddSingleton<ISendGridClient>(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<SendGridOptions>>().Value;
                var sendGridKey = options.Key;
                return new SendGridClient(sendGridKey);
            });

            services.AddSingleton(HtmlEncoder.Default);
        }
    }
}
