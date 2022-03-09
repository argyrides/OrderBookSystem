using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OrderBookSystemClient.Data.Entities
{
    [Table("BookTypes")]
    public class BookType
    {
        [Key, Required]
        public int BookTypes_id { get; set; }
        public string bookTypes_name { get; set; }

    }
}
