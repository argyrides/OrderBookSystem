using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OrderBookSystemClient.Data.Entities
{
    [Table("Users")]
    public class User 
    {
        [Key, Required]
        public int user_id { get; set; }

        [Required(ErrorMessage = "Username is Required!")]
        public string username { get; set; }

        [Required(ErrorMessage = "Password is Required!")]
        [DataType(DataType.Password)]
        public string password { get; set; }

        [Required(ErrorMessage = "Email is Required!")]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}")]
        [DataType(DataType.EmailAddress)]
        public string email { get; set; }


        [Required(ErrorMessage = "First name is Required!")]
        public string firstname { get; set; }


        [Required(ErrorMessage = "Last name is Required!")]
        public string lastname { get; set; }
        public DateTime last_login { get; set; }
        public bool is_locked { get; set; }
        public int login_tries { get; set; }
        public string role { get; set; }
        public bool is_approve { get; set;}

        public bool is_login { get; set; }

        public bool is_deleted { get; set; }
        public ICollection<OrderDetail> OrderDetailsList { get; set; }

    }
}
