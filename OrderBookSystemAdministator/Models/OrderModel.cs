using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderBookSystemAdministator.Models
{
    public class OrderModel
    {
        public int order_id { get; set; }
        public string username { get; set; }
        public string book_title { get; set; }
        public bool for_buy { get; set; }
        public bool for_loan { get; set; }
        public double price { get; set; }
        public int quantity { get; set; }

    }
}
