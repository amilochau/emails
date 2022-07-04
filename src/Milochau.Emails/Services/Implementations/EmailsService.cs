using Microsoft.Extensions.Options;
using Milochau.Emails.Sdk.Models;
using Milochau.Emails.DataAccess;
using Milochau.Emails.Options;
using Milochau.Emails.Services.EmailTemplates;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace Milochau.Emails.Services.Implementations
{
    public class EmailsService : IEmailsService
    {
        private readonly IEmailsDataAccess emailsDataAccess;
        private readonly IEmailTemplateFactory emailTemplateFactory;
        private readonly EmailsOptions options;

        public EmailsService(IEmailsDataAccess emailsDataAccess,
            IEmailTemplateFactory emailTemplateFactory,
            IOptionsSnapshot<EmailsOptions> options)
        {
            this.emailsDataAccess = emailsDataAccess;
            this.emailTemplateFactory = emailTemplateFactory;
            this.options = options.Value;
        }

        public async Task SendEmailAsync(Email email, CancellationToken cancellationToken)
        {
            // Format email
            FormatEmail(email);

            // Replace body thanks to email template
            var emailTemplate = emailTemplateFactory.Create(email.TemplateId);
            email.Body = emailTemplate.GetAsString(email);

            // Send email
            await emailsDataAccess.SendEmailAsync(email, cancellationToken);
        }

        internal void FormatEmail(Email email)
        {
            FormatRecipients(email);
            FormatReplyTo(email);
        }

        internal void FormatRecipients(Email email)
        {
            email.Tos = FormatRecipients(email.Tos);
            email.Ccs = FormatRecipients(email.Ccs, email.Tos.ToArray());
            email.Bccs = FormatRecipients(email.Bccs, email.Tos.Concat(email.Ccs).ToArray());
        }

        internal IList<EmailAddress> FormatRecipients(IList<EmailAddress> addresses, params EmailAddress[] includedAddresses)
        {
            var formattedAddresses = new List<EmailAddress>();

            foreach (var address in addresses)
            {
                if (string.IsNullOrWhiteSpace(address.Email))
                {
                    // The email address is empty
                    continue;
                }

                if (formattedAddresses.Any(x => x.Email.Equals(address.Email, System.StringComparison.OrdinalIgnoreCase))
                    || includedAddresses.Any(x => x.Email.Equals(address.Email, System.StringComparison.OrdinalIgnoreCase)))
                {
                    // The email address is already added as a recipient
                    continue;
                }

                if (options.AuthorizedRecipientHosts.Any()
                    && !options.AuthorizedRecipientHosts.Contains(new MailAddress(address.Email).Host))
                {
                    // The host is not authorized
                    continue;
                }

                formattedAddresses.Add(address);
            }

            return formattedAddresses;
        }

        internal void FormatReplyTo(Email email)
        {
            if (string.IsNullOrWhiteSpace(email.ReplyTo.Email))
                email.ReplyTo.Email = email.From.Email;
        }
    }
}
