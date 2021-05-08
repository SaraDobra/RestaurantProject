using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PizzaTime.Models
{
    public class Contact : BaseEntity
    {
        public string Name {get; set;}
        public string Email {get; set;}
        public string Subject {get; set;}
        public string Message {get; set;}
    }
}