using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PizzaTime.Models {
    public class Order : BaseEntity {
        public string Method { get; set; }
        public string Size { get; set; }
        public string Crust { get; set; }
        public int Quantity { get; set; }
        public int MenuItemID { get; set; }
        public MenuItem MenuItem { get; set; }
        public int UserID { get; set; }
        public User User { get; set; }
        public double Price { get; set; }
        public bool Favorite { get; set; }
        public bool Active { get; set; }
    }
}
