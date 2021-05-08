using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PizzaTime.Models {
    public class OrderListModel {
        public int ID { get; set; }
        public string Method { get; set; }
        public string Size { get; set; }
        public string Crust { get; set; }
        public int Quantity { get; set; }
        public MenuItem MenuItem { get; set; }
        public double Price { get; set; }
    }
}
