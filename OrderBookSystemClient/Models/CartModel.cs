using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderBookSystemClient.Models
{
    public class CartModel
    {
        public string book_name { get; set; }
        public int book_id { get; set; }
        public string book_author { get; set; }
        public string book_type { get; set; }
        public int user_id { get; set; }
        public bool for_loan { get; set; }
        public bool for_buy { get; set; }
        public double price { get; set; }
        public double total_price { get; set; }        
    }
}
