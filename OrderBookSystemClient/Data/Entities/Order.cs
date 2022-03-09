using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OrderBookSystemClient.Data.Entities
{
    [Table("Orders")]
    public class Order
    {
        [Key, Required]
        public int order_id { get; set; }
        public string order_status { get; set; }
        public DateTime order_date { get; set; }
        public DateTime? order_returning_date { get; set; }
        public double order_price { get; set; }
        public double order_total_price { get; set; }

        public ICollection<OrderDetail> OrderDetailsList { get; set; }
    }
}
