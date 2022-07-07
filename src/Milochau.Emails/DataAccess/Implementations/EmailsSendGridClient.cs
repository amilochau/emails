using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Milochau.Core.Cosmos.Helpers;
using Milochau.Emails.DataAccess.Helpers;
using Milochau.Emails.Options;
using Milochau.Emails.Sdk.Models;
using Polly;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Net;
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
                .Handle<SendGridException>()
                .RetryAsync((exception, count) =>
                {
                    logger.LogWarning(exception, $"Error with attempt #{count} to send email with SendGrid");
                });

            var response = await policy.ExecuteAsync((ctx) => SendEmailAsync(sendGridMessage, ctx), cancellationToken);

            // Track emails when succeeded
            var emailTracking = CreateTrackingEmail(email, response.StatusCode);
            await cosmosClient.CreateItemAsync(DatabaseName, CosmosClientConstants.TrackingContainerName, emailTracking, emailTracking.Id, logger, cancellationToken);
        }

        public async Task<Response> SendEmailAsync(SendGridMessage sendGridMessage, CancellationToken cancellationToken)
        {
            var response = await sendGridClient.SendEmailAsync(sendGridMessage, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new SendGridException();
            }

            return response;
        }

        public async Task<SendGridMessage> CreateSendGridMessageAsync(Email email, CancellationToken cancellationToken)
        {
            var sendGridMessage = new SendGridMessage
            {
                From = new SendGrid.Helpers.Mail.EmailAddress(email.From.Email, email.From.Name),
                HtmlContent = email.Body,
                Subject = email.Subject
            };

            foreach (var to in email.Tos)
            {
                sendGridMessage.AddTo(to.Email, to.Name);
            }

            foreach (var cc in email.Ccs)
            {
                sendGridMessage.AddCc(cc.Email, cc.Name);
            }

            foreach (var bcc in email.Bccs)
            {
                sendGridMessage.AddBcc(bcc.Email, bcc.Name);
            }

            if (!string.IsNullOrWhiteSpace(email.ReplyTo.Email))
            {
                sendGridMessage.ReplyTo = new SendGrid.Helpers.Mail.EmailAddress(email.ReplyTo.Email, email.ReplyTo.Name);
            }

            foreach (var attachment in email.Attachments)
            {
                var fileStream = await storageDataAccess.ReadToStreamAsync(attachment, cancellationToken);
                var fileName = attachment.GetNormalizedFileName();
                await sendGridMessage.AddAttachmentAsync(fileName, fileStream, null, null, null, cancellationToken);
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

        private static Entities.EmailTracking CreateTrackingEmail(Email email, HttpStatusCode statusCode)
        {
            return new Entities.EmailTracking
            {
                Id = Guid.NewGuid().ToString("N"),
                Tos = email.Tos,
                Ccs = email.Ccs,
                Bccs = email.Bccs,
                Subject = email.Subject,
                TemplateId = email.TemplateId,
                StatusCode = statusCode
            };
        }
    }
}
