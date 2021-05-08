using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PizzaTime.Models {
    public class OrderModel {
        [Display(Name = "METHOD")]
        [Required]
        public string Method { get; set; }
        [Display(Name = "SIZE")]
        [Required]
        public string Size { get; set; }
        [Display(Name = "CRUST")]
        [Required]
        public string Crust { get; set; }
        [Display(Name = "QTY")]
        [Required]
        [Range(0, Double.PositiveInfinity)]
        public int Quantity { get; set; }
        [Display(Name = "FOOD")]
        [Required]
        public int MenuItem { get; set; }
        public double Price { get; set; }
    }
}
