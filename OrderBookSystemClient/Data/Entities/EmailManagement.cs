using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OrderBookSystemClient.Data.Entities
{
    [Table("EmailManagement")]
    public class EmailManagement
    {
        [Key, Required]
        public int emailmanagement_id { get; set; }

        [ForeignKey("Z_email_id")]
        public int email_id { get; set; }

        [ForeignKey("Z_user_id")]
        public int user_id { get; set; }

        public virtual User Z_user_id { get; set; }
        public virtual Email Z_email_id { get; set; }
    }
}
