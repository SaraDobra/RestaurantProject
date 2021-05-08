using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PizzaTime.Models {
    public class ReserveModel : BaseEntity{
        [Display(Name = "Full Name")]
        [Required]
        public string Name { get; set; }
        [Display(Name = "Email")]
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Display(Name = "Phone num.")]
        [Required]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }
        [Display(Name = "Pick a table")]
        [Required]
        public int TableID { get; set; }
        public Table Table { get; set; }
    }
}
