using Milochau.Emails.Helpers;
using Milochau.Emails.Sdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Milochau.Emails.DataAccess.Entities
{
    public class Email : IEntity
    {
        public string Id { get; set; } = null!;

        public List<EmailAddress> Tos { get; set; } = new List<EmailAddress>();

        public List<EmailAddress> Ccs { get; set; } = new List<EmailAddress>();

        public List<EmailAddress> Bccs { get; set; } = new List<EmailAddress>();

        public string Subject { get; set; } = null!;
        public string? TemplateId { get; set; }
    }
}
