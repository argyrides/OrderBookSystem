using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OrderBookSystemClient.Data.Entities
{
    [Table("BooksUsers")]
    public class BookUser
    {
        [Key, Required]
        public int booksusers_id { get; set; }

        [ForeignKey("Z_book_id")]
        public int book_id { get; set; }

        [ForeignKey("Z_user_id")]
        public int user_id { get; set; }

        public bool for_loan { get; set; }

        public bool for_buy { get; set; }

        public DateTime loan_returning_date { get; set; }

        public bool order_is_send { get; set; }

        public int quantity { get; set; }

        public virtual Book Z_book_id { get; set; }

        public virtual User Z_user_id { get; set; }

        public ICollection<OrderDetail> OrderDetailsList { get; set; }

    }
}
