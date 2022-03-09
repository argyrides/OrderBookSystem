using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderBookSystemAdministator.Models
{
    public class LogModel
    {
        public int no { get; set; }

        public DateTime order_date { get; set; }

        public string book_title { get; set; }

        public string book_author { get; set; }

        public string book_type { get; set; }

        public DateTime returning_date { get; set; }

    }
}
