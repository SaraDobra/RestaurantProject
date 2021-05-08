using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PizzaTime.Models;
using RealEstateCMS.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PizzaTime.Controllers {
    public class HomeController : Controller {
        private readonly ILogger<HomeController> _logger;

        private readonly AppDbContext _context;

        private User ActiveUser { get; set; }
        public User activeuser {
            get { return _context.Users.Where(u => u.ID == HttpContext.Session.GetInt32("UserId")).FirstOrDefault(); }
        }

        public HomeController(ILogger<HomeController> logger, AppDbContext context) {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index() {
            ViewBag.Msg = null;
            ViewBag.ActiveUser = activeuser;
            ViewBag.OrdersNum = 0;
            if (ViewBag.ActiveUser != null) {
                ViewBag.OrdersNum = _context.Orders.Count(o => o.UserID == activeuser.ID && o.Active == true);
            }
            return View();
        }

        public IActionResult Home() {
            ViewBag.ActiveUser = activeuser;

            if (ViewBag.ActiveUser == null) {
                return RedirectToAction("Login", "User");
            }
            ViewBag.Msg = null;
            ViewBag.OrdersNum = _context.Orders.Where(o => o.UserID == activeuser.ID && o.Active == true).ToList().Count;
            return View();
        }

        [HttpGet("Order")]
        public IActionResult Create() {
            ViewBag.ActiveUser = activeuser;
            if (ViewBag.ActiveUser == null) {
                return RedirectToAction("Login", "User");
            }
            ViewBag.Pizza = _context.MenuItems.ToList();
            ViewBag.OrdersNum = _context.Orders.Where(o => o.UserID == activeuser.ID && o.Active == true).ToList().Count;
            return View();
        }

        [HttpGet("Favorite")]
        public IActionResult FavoriteOrder() {
            ViewBag.ActiveUser = activeuser;
            if (ViewBag.ActiveUser == null) {
                return RedirectToAction("Login", "User");
            }
            ViewBag.Pizza = _context.MenuItems.ToList();
            var favPizza = _context.Orders.Where(o => o.UserID == activeuser.ID).Include(p => p.MenuItem).ToList();
            ViewBag.FavPizza = favPizza.Where(p => p.Favorite == true).FirstOrDefault();
            ViewBag.OrdersNum = _context.Orders.Where(o => o.UserID == activeuser.ID && o.Active == true).ToList().Count;
            if (ViewBag.FavPizza == null) {
                ViewBag.Msg = "You don't a favorite pizza!";
                ViewBag.OrdersNum = _context.Orders.Where(o => o.UserID == activeuser.ID && o.Active == true).ToList().Count;
                return View("Index");
            }

            return View("FavoriteOrder");
        }

        [HttpPost("Order/Check")]
        public IActionResult CheckOrder(OrderModel orderModel) {
            ModelState.Remove("Price");
            if (ModelState.IsValid) {
                return RedirectToAction("Details", orderModel);
            }
            ViewBag.Pizza = _context.MenuItems.ToList();
            ViewBag.OrdersNum = _context.Orders.Where(o => o.UserID == activeuser.ID && o.Active == true).ToList().Count;
            return View("Create");
        }

        [HttpPost("Order/Create")]
        public IActionResult CreateOrder(OrderModel orderModel) {
            if (ModelState.IsValid) {
                Order order = new Order() {
                    Method = orderModel.Method,
                    Size = orderModel.Size,
                    Crust = orderModel.Crust,
                    Quantity = orderModel.Quantity,
                    MenuItemID = _context.MenuItems.FirstOrDefault(p => p.ID == orderModel.MenuItem).ID,
                    UserID = activeuser.ID,
                    Favorite = false,
                    Active = true,
                };
                switch (orderModel.Size) {
                    case "Small":
                        order.Price = 7.99 * order.Quantity;
                        break;
                    case "Medium":
                        order.Price = 9.99 * order.Quantity;
                        break;
                    case "Large":
                        order.Price = 12.99 * order.Quantity;
                        break;
                    case "Extra Large":
                        order.Price = 15.99 * order.Quantity;
                        break;
                }
                _context.Orders.Add(order);
                _context.SaveChanges();

                var lastOrder = _context.Orders.OrderBy(o => o.UpdatedAt).First();
                ViewBag.ActiveUser = activeuser;
                ViewBag.OrdersNum = _context.Orders.Where(o => o.UserID == activeuser.ID && o.Active == true).ToList().Count;
                return RedirectToAction("Details", orderModel);
            }
            return View("Create");
        }

        [Route("Reserve")]
        public IActionResult Reserve() {
            var avaliableTables = _context.Tables.Where(t => t.Reserved == false).ToList();
            ViewBag.ListItems = avaliableTables;
            return View();
        }

        [HttpPost("Reserve/Check")]
        public IActionResult ReserveTable(ReserveModel reserveModel) {
            if (reserveModel.TableID == 0) {
                ModelState.AddModelError(string.Empty, "Pick a table");
            }
            if (ModelState.IsValid) {

                _context.ReserveModels.Add(reserveModel);
                _context.SaveChanges();

                var table = _context.Tables.FirstOrDefault(t => t.ID == reserveModel.TableID);
                table.Reserved = true;

                _context.Update(table);
                _context.SaveChanges();
                ViewBag.SuccessMsg = $"Faleminderit {reserveModel.Name} Tavolina numer {table.TableNumber} u rezervua me sekses!";
                ViewBag.ActiveUser = activeuser;
                return View("Index");
            }
            var avaliableTables = _context.Tables.Where(t => t.Reserved == false).ToList();
            ViewBag.ListItems = avaliableTables;
            return View("Reserve");
        }

        [HttpPost("/purchase")]
        public IActionResult Purchase(OrderModel orderModel) {
            var order = _context.Orders.FirstOrDefault(o => o.UserID == activeuser.ID
                                                && o.Method == orderModel.Method
                                                && o.MenuItemID == orderModel.MenuItem
                                                && o.Price == orderModel.Price
                                                && o.Quantity == orderModel.Quantity
                                                && o.Crust == orderModel.Crust
                                                && o.Active == true);
            order.Active = false;
            _context.Update(order);
            _context.SaveChanges();
            return RedirectToAction("ActiveOrders");
        }

        [HttpGet("/Order/Details")]
        public IActionResult Details(OrderModel orderModel) {
            ViewBag.ActiveUser = activeuser;
            if (ViewBag.ActiveUser == null) {
                return RedirectToAction("Index");
            }

            ViewBag.Order = orderModel;
            var pizza = _context.MenuItems.FirstOrDefault(p => p.ID == orderModel.MenuItem);
            ViewBag.Pizza = pizza;
            var price = 0.00;
            switch (orderModel.Size) {
                case "Small":
                    price = 7.99;
                    break;
                case "Medium":
                    price = 9.99;
                    break;
                case "Large":
                    price = 12.99;
                    break;
                case "Extra Large":
                    price = 15.99;
                    break;
            }
            ViewBag.Price = price;
            ViewBag.PriceQty = price * orderModel.Quantity;
            ViewBag.ActiveUser = activeuser;
            ViewBag.OrdersNum = _context.Orders.Where(o => o.UserID == activeuser.ID && o.Active == true).ToList().Count;
            return View();
        }

        [HttpGet("/Order/Detail")]
        public IActionResult DetailsID(int id) {
            ViewBag.ActiveUser = activeuser;
            if (ViewBag.ActiveUser == null) {
                return RedirectToAction("Index");
            }
            var order = _context.Orders.FirstOrDefault(o => o.ID == id);
            OrderModel orderModel = new OrderModel() {
                Crust = order.Crust,
                Method = order.Method,
                MenuItem = order.MenuItemID,
                Price = order.Price,
                Quantity = order.Quantity,
                Size = order.Size
            };
            ViewBag.Order = orderModel;
            var pizza = _context.MenuItems.FirstOrDefault(p => p.ID == orderModel.MenuItem);
            ViewBag.Pizza = pizza;
            var price = 0.00;
            switch (orderModel.Size) {
                case "Small":
                    price = 7.99;
                    break;
                case "Medium":
                    price = 9.99;
                    break;
                case "Large":
                    price = 12.99;
                    break;
                case "Extra Large":
                    price = 15.99;
                    break;
            }
            ViewBag.Price = price;
            ViewBag.PriceQty = price * orderModel.Quantity;
            ViewBag.ActiveUser = activeuser;
            ViewBag.OrdersNum = _context.Orders.Where(o => o.UserID == activeuser.ID && o.Active == true).ToList().Count;
            return View("Details");
        }

        [HttpGet("Orders")]
        public IActionResult ActiveOrders() {
            ViewBag.ActiveUser = activeuser;
            if (ViewBag.ActiveUser == null) {
                return RedirectToAction("Index");
            }
            var allOrders = _context.Orders.Where(o => o.UserID == activeuser.ID && o.Active == true).ToList();
            ViewBag.OrdersNum = _context.Orders.Where(o => o.UserID == activeuser.ID && o.Active == true)
                                                .Include(o => o.MenuItem).ToList().Count;
            var allOrderModel = new List<OrderListModel>();
            foreach (var item in allOrders) {
                OrderListModel order = new OrderListModel() {
                    ID = item.ID,
                    Crust = item.Crust,
                    Method = item.Method,
                    MenuItem = item.MenuItem,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    Size = item.Size
                };
                allOrderModel.Add(order);
            }
            return View("AllOrders", allOrderModel);
        }

        [HttpGet("/Surprise")]
        public IActionResult Surprise() {
            ViewBag.ActiveUser = activeuser;
            if (ViewBag.ActiveUser == null) {
                return RedirectToAction("Login", "User");
            }
            ViewBag.Pizza = _context.MenuItems.ToList();
            var ordersNum = _context.Orders.Where(o => o.UserID == activeuser.ID && o.Active == true).ToList().Count;
            ViewBag.OrdersNum = ordersNum;
            if (ordersNum == 0) {
                ViewBag.SurpriseMsg = "You don't have any orders so I can pick one!";
                ViewBag.OrdersNum = _context.Orders.Where(o => o.UserID == activeuser.ID && o.Active == true).ToList().Count;
                return View("Index");
            }
            Random rnd = new Random();
            var allOrders = _context.Orders.ToArray();
            ViewBag.RandomOrder = allOrders[rnd.Next(0, allOrders.Length)];
            return View("Surprise");
        }

        [HttpGet("Contact")]
        public IActionResult Contact(){
            ViewBag.ActiveUser = activeuser;

            if (ViewBag.ActiveUser == null) {
                return RedirectToAction("Login", "User");
            }
            ViewBag.Msg = null;
            ViewBag.OrdersNum = _context.Orders.Where(o => o.UserID == activeuser.ID && o.Active == true).ToList().Count;
            return View();
        }

        // [HttpPost("/contact")]
        // public IActionResult Contact(Contact contact) {
        //     var c = _context.Contact.FirstOrDefault(c => c.Name == contact.Name
        //                                         && c.Email == contact.Email
        //                                         && c.Subject == contact.Subject
        //                                         && c.Message == contact.Message);

        //     _context.SaveChanges();
        //     return View("Index");
        // }

        public IActionResult Privacy() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
