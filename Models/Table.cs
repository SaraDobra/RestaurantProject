using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PizzaTime.Models {
    public class Table : BaseEntity {
        public int NumberOfSeats { get; set; }
        public bool Reserved { get; set; }
        public int TableNumber { get; set; }
    }
}
