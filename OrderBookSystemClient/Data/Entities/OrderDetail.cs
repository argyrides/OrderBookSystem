using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OrderBookSystemClient.Data.Entities
{

    [Table("OrdersDetails")]

    public class OrderDetail
    {
        [Key, Column(Order = 0)]
        [ForeignKey("Z_order_id")]
        public int order_id { get; set; }

        [Key, Column(Order = 1)]
        [ForeignKey("Z_user_id")]
        public int user_id { get; set; }


        [Key, Column(Order = 2)]
        [ForeignKey("Z_bookuser_id")]
        public int booksusers_id { get; set; }
       
        public virtual BookUser Z_bookuser_id { get; set; }
        public virtual User Z_user_id { get; set; }
        public virtual Order Z_order_id { get; set; }
    }
}
