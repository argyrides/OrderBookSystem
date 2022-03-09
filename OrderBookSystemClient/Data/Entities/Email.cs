using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OrderBookSystemClient.Data.Entities
{
    [Table("Emails")]
    public class Email
    {
        [Key, Required]
        public int email_id { get; set; }
        public string email_content { get; set; }

        [ForeignKey("Z_email_id"),Required]
        public int type_id { get; set; }

        public virtual EmailType Z_email_id { get; set; }
    }
}
