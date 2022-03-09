using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OrderBookSystemClient.Data.Entities
{
    [Table("Books")]
    public class Book
    {
        [Key, Required]
        public int book_id { get; set; }
        public string book_author { get; set; }
        public string book_title { get; set; }
        public string book_image { get; set; }
        public double book_loan_price { get; set;}
        public double book_buy_price { get; set; }
        public int book_quantity { get; set; }
        public bool book_loan_availability { get; set; }
        public bool book_buy_availability { get; set; }

        [ForeignKey("Z_BookType")]
        public int bookTypes_id { get; set; }

        public bool book_has_deleted { get; set; }

        public DateTime insert_date { get; set; }
        public virtual BookType Z_BookType { get; set; }


    }
}
