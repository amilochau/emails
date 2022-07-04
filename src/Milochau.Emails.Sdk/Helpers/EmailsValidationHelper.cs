using Milochau.Emails.Sdk.Models;
using System.Collections.Generic;
using System.Linq;

namespace Milochau.Emails.Sdk.Helpers
{
    /// <summary>Validation helper for emails</summary>
    public class EmailsValidationHelper : IEmailsValidationHelper
    {
        /// <summary>Maximum recipients supported per email</summary>
        /// <remarks>If you want to send your email to more that <see cref="MaximumRecipents"/> recipients, create more emails with fewer recipients.</remarks>
        public const int MaximumRecipents = 1000;

        /// <summary>Validate model before sending email</summary>
        public IEnumerable<string> ValidateEmail(Email email)
        {
            return ValidateBasics(email);
        }

        private static IEnumerable<string> ValidateBasics(Email email)
        {
            if (string.IsNullOrWhiteSpace(email.From.Email))
            {
                yield return "A sender email address must be included.";
            }

            if (!email.Tos.Where(x => !string.IsNullOrWhiteSpace(x.Email)).Any())
            {
                yield return "A recipient must be included.";
            }

            if (string.IsNullOrWhiteSpace(email.Subject))
            {
                yield return "A subject must be included.";
            }

            if (string.IsNullOrWhiteSpace(email.Body))
            {
                yield return "A body must be included.";
            }

            var recipients = email.Tos.Count + email.Ccs.Count + email.Bccs.Count;
            if (recipients > MaximumRecipents)
            {
                yield return $"You reached the maximum number of recipients ({recipients} > {MaximumRecipents}).";
            }

            foreach (var attachment in email.Attachments)
            {
                if (string.IsNullOrWhiteSpace(attachment.GetNormalizedFileName()))
                {
                    yield return $"Attachments must have a non-whitespace file name, with at least one non-rendered character.";
                }
            }
        }
    }
}
