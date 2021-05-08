using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaTime.Models;
using RealEstateCMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateCMS.Controllers {
    public class UserController : Controller {
        private readonly AppDbContext _context;
        private User ActiveUser { get; set; }
        public User activeuser {
            get { return _context.Users.Where(u => u.ID == HttpContext.Session.GetInt32("UserId")).FirstOrDefault(); }
        }
        public UserController(AppDbContext context) {
            _context = context;
        }

        [HttpGet("register")]
        public IActionResult Register() {
            //if you are logged in,don't show register view
            if (HttpContext.Session.GetInt32("UserId") == null) {
                ViewBag.State = _context.States.ToList();
                return View("RegisterForm");
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost("/User/register")]
        public IActionResult RegisterUser(RegisterModel registerUser) {
            //if you are logged in,don't show register view
            if (HttpContext.Session.GetInt32("UserId") != null) {
                return RedirectToAction("Index", "Home");
            }
            if (registerUser.Password != registerUser.ConfirmPassword) {
                ModelState.AddModelError("Password", "Confirmation Password must match the Passsword");
                return View("RegisterForm");
            }
            User checkIfExists = _context.Users.FirstOrDefault(u => u.Email == registerUser.Email);
            if (checkIfExists != null) {
                ModelState.AddModelError("Email", "This email is been used once before!");
                return View("RegisterForm");
            }
            if (ModelState.IsValid) {

                User user = new User() {
                    FirstName = registerUser.FirstName,
                    LastName = registerUser.LastName,
                    Address = registerUser.Address,
                    City = registerUser.City,
                    StateID = _context.States.FirstOrDefault(s => s.ID == registerUser.State).ID,
                    Email = registerUser.Email
                };
                PasswordHasher<User> hasher = new PasswordHasher<User>();
                user.Password = hasher.HashPassword(user, registerUser.Password);
                _context.Users.Add(user);
                _context.SaveChanges();

                User lastUser = _context.Users.OrderBy(u => u.UpdatedAt).Last();
                HttpContext.Session.SetInt32("UserId", lastUser.ID);
                return RedirectToAction("Login", "User");
            }
            return View("RegisterForm");
        }

        [HttpGet("login")]
        public IActionResult Login() {
            //if you are logged in,don't show login view
            if (HttpContext.Session.GetInt32("UserId") == null) {
                return View("LoginForm");
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost("/User/login")]
        public IActionResult LoginUser(LoginModel loginUser) {
            //if you are logged in,don't show login view
            if (HttpContext.Session.GetInt32("UserId") != null) {
                return RedirectToAction("Index", "Home");
            }
            PasswordHasher<LoginModel> hasher = new PasswordHasher<LoginModel>();
            User user = _context.Users.FirstOrDefault(u => u.Email == loginUser.email);
            if (user == null || loginUser.password == null) {
                ModelState.AddModelError("Email", "Invalid Email/Password");
                return View("LoginForm");
            } else if (hasher.VerifyHashedPassword(loginUser, user.Password, loginUser.password) != PasswordVerificationResult.Success) {
                ModelState.AddModelError("Email", "Invalid Email/Password");
                return View("Loginform");
            }
            if (!ModelState.IsValid) {
                return View("LoginForm");
            }
            HttpContext.Session.SetInt32("UserId", user.ID);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("Update")]
        public IActionResult Edit() {
            if (HttpContext.Session.GetInt32("UserId") != null) {
                ViewBag.State = _context.States.ToList();
                ViewBag.ActiveUser = activeuser;
                var allOrders = _context.Orders.Where(o => o.UserID == activeuser.ID && o.Active == false).Include(o => o.MenuItem).ToList();
                ViewBag.OrdersNum = allOrders.Count;
                ViewBag.Order = allOrders; 
                return View();
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost("/User/update")]
        public IActionResult EditUser(RegisterModel registerUser) {
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");
            if (ModelState.IsValid) {


                User user = _context.Users.FirstOrDefault(u => u.ID == activeuser.ID);
                user.FirstName = registerUser.FirstName;
                user.LastName = registerUser.LastName;
                user.Address = registerUser.Address;
                user.City = registerUser.City;
                user.StateID = _context.States.FirstOrDefault(s => s.ID == registerUser.State).ID;
                user.Email = registerUser.Email;
                user.CreatedAt = activeuser.CreatedAt;
                user.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                User lastUser = _context.Users.OrderBy(u => u.UpdatedAt).Last();
                HttpContext.Session.SetInt32("UserId", lastUser.ID);
                return RedirectToAction("Index", "Home");
            }
            ViewBag.State = _context.States.ToList();
            ViewBag.ActiveUser = activeuser;
            var allOrders = _context.Orders.Where(o => o.UserID == activeuser.ID && o.Active == false).Include(o => o.MenuItem).ToList();
            ViewBag.OrdersNum = allOrders.Count;
            ViewBag.Order = allOrders; 
            return View("Edit");
        }

        [HttpGet("/Favorite/{id}")]
        public IActionResult Favorite(int id) {

            var favOrders = _context.Orders.FirstOrDefault(o => o.UserID == activeuser.ID && o.Active == false && o.Favorite == true);
            if (favOrders != null) {
                favOrders.Favorite = false;
                _context.SaveChanges();
            }

            var order = _context.Orders.FirstOrDefault(o => o.ID == id);
            order.Favorite = true;
            _context.SaveChanges();
            ViewBag.State = _context.States.ToList();
            ViewBag.ActiveUser = activeuser;
            var allOrders = _context.Orders.Where(o => o.UserID == activeuser.ID && o.Active == false).Include(o => o.MenuItem).ToList();
            ViewBag.OrdersNum = allOrders.Count;
            ViewBag.Order = allOrders;

            return View("Edit");
        }

        [HttpGet("/User/logout")]
        public IActionResult LogOut() {
            HttpContext.Session.Clear();
            ViewBag.ActiveUser = ActiveUser;
            return RedirectToAction("Index", "Home");
        }
    }
}
