using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OrderBookSystemClient.Data.Entities
{
    [Table("EmailTypes")]
    public class EmailType
    {
        [Key, Required]
        public int type_id { get; set; }
        public string type_title { get; set; }
    }
}
