using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Milochau.Emails.DataAccess.Helpers;
using Milochau.Emails.Options;
using Milochau.Emails.Sdk.Models;
using Polly;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Milochau.Emails.DataAccess.Implementations
{
    internal class EmailsSendGridClient : IEmailsDataAccess
    {
        private readonly ISendGridClient sendGridClient;
        private readonly IStorageDataAccess storageDataAccess;
        private readonly CosmosClient cosmosClient;
        private readonly IOptions<DatabaseOptions> databaseOptions;
        private readonly ILogger<EmailsSendGridClient> logger;

        public string DatabaseName => databaseOptions.Value.DatabaseName;

        public EmailsSendGridClient(ISendGridClient sendGridClient,
            IStorageDataAccess storageDataAccess,
            CosmosClient cosmosClient,
            IOptions<DatabaseOptions> databaseOptions,
            ILogger<EmailsSendGridClient> logger)
        {
            this.sendGridClient = sendGridClient;
            this.storageDataAccess = storageDataAccess;
            this.cosmosClient = cosmosClient;
            this.databaseOptions = databaseOptions;
            this.logger = logger;
        }

        public async Task SendEmailAsync(Email email, CancellationToken cancellationToken)
        {
            var sendGridMessage = await CreateSendGridMessageAsync(email, cancellationToken);

            var policy = Policy
                .Handle<Exception>()
                .RetryAsync((exception, count) =>
                {
                    logger.LogWarning(exception, $"Error with attempt #{count} to send email with SendGrid");
                });

            await policy.ExecuteAsync((ctx) => SendEmailAsync(sendGridMessage, ctx), cancellationToken);

            var entity = new Entities.Email
            {
                Id = Guid.NewGuid().ToString("N"),
                Tos = email.Tos,
                Ccs = email.Ccs,
                Bccs = email.Bccs,
                Subject = email.Subject,
                TemplateId = email.TemplateId
            };
            await cosmosClient.CreateItemAsync(DatabaseName, CosmosClientConstants.EmailsContainerName, entity, entity.Id, logger, cancellationToken);
        }

        public async Task SendEmailAsync(SendGridMessage sendGridMessage, CancellationToken cancellationToken)
        {
            await sendGridClient.SendEmailAsync(sendGridMessage, cancellationToken);
        }

        public async Task<SendGridMessage> CreateSendGridMessageAsync(Email email, CancellationToken cancellationToken)
        {
            var sendGridMessage = new SendGridMessage
            {
                From = new SendGrid.Helpers.Mail.EmailAddress(email.From.Email, email.From.Name),
                HtmlContent = email.Body,
                Subject = email.Subject
            };

            email.Tos.ForEach(x => sendGridMessage.AddTo(x.Email, x.Name));

            if (email.Ccs != null)
                email.Ccs.ForEach(x => sendGridMessage.AddCc(x.Email, x.Name));

            if (email.Bccs != null)
                email.Bccs.ForEach(x => sendGridMessage.AddBcc(x.Email, x.Name));

            if (email.ReplyTo != null)
                sendGridMessage.ReplyTo = new SendGrid.Helpers.Mail.EmailAddress(email.ReplyTo.Email, email.ReplyTo.Name);

            if (email.Attachments != null)
            {
                foreach (var attachment in email.Attachments)
                {
                    var fileStream = await storageDataAccess.ReadToStreamAsync(attachment, cancellationToken);
                    await sendGridMessage.AddAttachmentAsync(attachment.PublicFileName ?? attachment.FileName, fileStream, null, null, null, cancellationToken);
                }
            }

            SetImportance(sendGridMessage, email);
            SetPriority(sendGridMessage, email);

            return sendGridMessage;
        }

        public void SetImportance(SendGridMessage sendGridMessage, Email email)
        {
            switch (email.Importance)
            {
                case ImportanceType.Low:
                    sendGridMessage.AddHeader("Importance", "low");
                    break;
                case ImportanceType.High:
                    sendGridMessage.AddHeader("Importance", "high");
                    break;
                case ImportanceType.Normal:
                default:
                    break;
            }
        }

        public void SetPriority(SendGridMessage sendGridMessage, Email email)
        {
            switch (email.Priority)
            {
                case PriorityType.NonUrgent:
                    sendGridMessage.AddHeader("Priority", "non-urgent");
                    break;
                case PriorityType.Urgent:
                    sendGridMessage.AddHeader("Priority", "urgent");
                    break;
                case PriorityType.Normal:
                default:
                    break;
            }
        }
    }
}
