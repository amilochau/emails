using Milochau.Emails.Sdk.Models;
using Milochau.Finance.Helpers;
using System;
using System.Collections.Generic;

namespace Milochau.Emails.DataAccess.Entities
{
    public class EmailTracking : IEntity<string>
    {
        public string Id { get; set; } = null!;

        public IList<EmailAddress> Tos { get; set; } = new List<EmailAddress>();

        public IList<EmailAddress> Ccs { get; set; } = new List<EmailAddress>();

        public IList<EmailAddress> Bccs { get; set; } = new List<EmailAddress>();

        public string Subject { get; set; } = null!;
        public string? TemplateId { get; set; }

        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    }
}
