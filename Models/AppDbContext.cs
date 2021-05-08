using Microsoft.EntityFrameworkCore;
using PizzaTime.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateCMS.Models {
    public class AppDbContext : DbContext {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<ReserveModel> ReserveModels { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Contact> Contact { get; set; }
    }
}
