using e_shop.Models;
using e_shop.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Reflection;

namespace e_shop.Controllers
{
    public class HomeController : Controller
    {
        public static List<CartItem> CartData = new List<CartItem>();

        private readonly ILogger<HomeController> _logger;
        private Customerwebsite1Context db;
        private Customerwebsite1Context _context;
        public HomeController(ILogger<HomeController> logger, Customerwebsite1Context context)
        {
            _logger = logger;
            db = new Customerwebsite1Context();
            _context = context;

        }
        //for userpage

        public IActionResult Index()
        {
            var categories = db.Categories.ToList();
            //var brands = db.Brands.ToList();
            var products = db.Products
                .Include(x => x.CategoryF)
               //.Include(x=>x.BrandF)
                .ToList();

            var viewModel = new IndexViewModel
            {
                Categories = categories,
                //Brands = brands,
                Products = products
            };

            return View(viewModel);
        }

        //for admin page
        public IActionResult Admin()
        {
            return View();
        }

        //for login page
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult LoginCheck(string txtEmail, string txtPass)
        {
            // Try to find the user in Admins table
            var admin = db.Admins.FirstOrDefault(x => x.Email == txtEmail && x.Passwor == txtPass);
            if (admin != null)
            {
                TempData["Title"] = "Login";
                TempData["Message"] = "Admin login successful";
                TempData["Icon"] = "success";
                return Redirect("/Home/Admin");
            }

            // Try to find the user in Customers table
            var user = db.Customers.FirstOrDefault(x => x.Email == txtEmail && x.Password == txtPass);
            if (user != null)
            {
                TempData["Title"] = "Login";
                TempData["Message"] = "User login successful";
                TempData["Icon"] = "success";
                return Redirect("/Home/Index");
            }

            // If neither found
            TempData["Title"] = "Error";
            TempData["Message"] = "Email or Password is incorrect";
            TempData["Icon"] = "error";
            return Redirect("/Home/Login");
        }




        //for register
 
        public IActionResult Register(string txtName, string txtEmail, string txtPass, string txtCPass, bool acceptTerms)
        {
            if (acceptTerms==true)
            {
                TempData["Title"] = "Error";
                TempData["Message"] = "You must accept the terms and conditions to register.";
                TempData["Icon"] = "warning";
                return Redirect("/Home/Login");
            }


            if (txtPass != txtCPass)
            {
                TempData["Title"] = "Error";
                TempData["Message"] = "Passwords do not match";
                TempData["Icon"] = "warning";
                return Redirect("/Home/Login");
            }

            var existingUser = db.Customers.FirstOrDefault(x => x.Email == txtEmail);
            if (existingUser != null)
            {
                TempData["Title"] = "Already Exists!";
                TempData["Message"] = "Email already taken";
                TempData["Icon"] = "warning";
                return Redirect("/Home/Login");
            }

            var newUser = new Customer
            {
                FullName = txtName,
                Email = txtEmail,
                Password = txtPass,
                Role = "User"
            };

            db.Customers.Add(newUser);
            db.SaveChanges();

            TempData["Title"] = "Account Registered";
            TempData["Message"] = "Account has been created successfully";
            TempData["Icon"] = "success";

            return Redirect("/Home/Login");
        }
    
        
        [HttpPost]
        [Authorize(Roles = "Admin")] // only if using ASP.NET Identity
        public IActionResult AddAdmin(string email, string password)
        {
            var existingAdmin = db.Admins.FirstOrDefault(x => x.Email == email);
            if (existingAdmin != null)
            {
                TempData["Message"] = "Admin with this email already exists.";
                return RedirectToAction("Dashboard");
            }

            var newAdmin = new Admin
            {
                Email = email,
                Passwor = password // hash this!
            };

            db.Admins.Add(newAdmin);
            db.SaveChanges();

            TempData["Message"] = "New Admin added successfully.";
            return RedirectToAction("Dashboard");
        }


        //for contact
        public IActionResult Contact()
        {
            return View();
        }

        //for checkout
         public ActionResult Checkout()
         {
         // Example cart - replace this with your actual cart from session or database
                 var cartItems = new List<CartItemViewModel>
                     {
            new CartItemViewModel { ProductName = "Item A", Price = 10.00m, Quantity = 2 },
            new CartItemViewModel { ProductName = "Item B", Price = 15.50m, Quantity = 1 }
                       };

             return View(cartItems);
          }

        //for cart
        public IActionResult Cart()
        {
            return View(CartData);
        }


        //for add to cart
        public IActionResult AddToCart(int id)
        {

            var p = db.Products.Include(x => x.CategoryF).FirstOrDefault(x => x.ProductId == id);

            if (CheckForExistingItem(id) == -1) {
                CartData.Add(new CartItem
                {
                    product = p,
                    quantity = 1
                });
            }
            else
            {
                CartData[CheckForExistingItem(id)].quantity++;
            }


            return RedirectToAction("Cart");
        }

        public ActionResult Remove(int id)
        {
            if (CheckForExistingItem(id) != -1)
            {
              CartData.RemoveAt(CheckForExistingItem(id));
            }
            return RedirectToAction("Cart");

        }
        int CheckForExistingItem(int id) 
        {
            int FoundIndex = -1;
            for (int i = 0; i < CartData.Count; i++)
            {
                if (id == CartData[i].product.ProductId)
                {
                    FoundIndex = i;
                }
            }
            return FoundIndex;
        }



        //for Detail
        public IActionResult Details(int id)
        {
            var product = db.Products.Include(x => x.CategoryF).FirstOrDefault(x=>x.ProductId== id);
            return View(product);
        }




        public IActionResult Products(int? id,int? brand)
        {
            //Displaying brands and categories on side
            ViewBag.Cats =db.Categories.ToList();
            //ViewBag.Brands =db.Brands.ToList();


            //filter category
            var Products = db.Products.Include(x=>x.CategoryF).ToList();
            if (id!=null)
            {
                return View(Products.Where(x=>x.CategoryFid==id));
            }

            //filter brand
            //if (brand != null)
            //{
            //    return View(Products.Where(x => x.BrandFid == brand));
            //}

            return View(Products);
           
        }

        //for categories
        public IActionResult Categories()
        {
           
			var Categories = db.Categories.ToList();
			return View(Categories);
        }

		//for brands
        //public IActionResult Brands()
        //{

        //    var brands = db.Brands.ToList();
        //    return View(brands);
        //}

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}


