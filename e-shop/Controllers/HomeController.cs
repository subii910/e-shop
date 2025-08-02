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
                //AppState.LoggedInCustomer = user;
                return RedirectToAction("Index", "Home");
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
        public IActionResult Checkout()
        {
            // Check if user is logged in
            var userEmail = User.Identity?.Name;

            CheckoutViewModel model = new CheckoutViewModel
            {
                CartItems = HomeController.CartData ?? new List<CartItem>()
            };

            if (!string.IsNullOrEmpty(userEmail))
            {
                // Fetch user info from DB using email
                var user = db.Customers.FirstOrDefault(u => u.Email == userEmail);
                if (user != null)
                {
                    model.FullName = user.FullName;
                    model.Email = user.Email;
                    model.Phone = user.Phone;
                    model.Address = user.Address;
                }
            }

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(CheckoutViewModel model)
        {
            // Check if CartData has items
            if (HomeController.CartData == null || !HomeController.CartData.Any())
            {
                ModelState.AddModelError("", "Your cart is empty. Please add items before proceeding to payment.");
                return View(model); // Return the same checkout view with error
            }

            // Validate form
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Store checkout details temporarily for Payment view
            TempData["FullName"] = model.FullName;
            TempData["Email"] = model.Email;
            TempData["Phone"] = model.Phone;
            TempData["Address"] = model.Address;

            return RedirectToAction("Payment");
        }


        public IActionResult Payment()
        {
            ViewBag.TotalAmount = CartData.Sum(i => i.product.Srice * i.quantity);
            return View();
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
        public IActionResult PlaceOrder()
        {
            if (!CartData.Any())
            {
                TempData["Title"] = "Cart Empty";
                TempData["Message"] = "Your cart is empty. Add items before placing an order.";
                TempData["Icon"] = "warning";
                return RedirectToAction("Cart");
            }

            // Simulate a logged-in customer (replace this with actual logic)
            var user = db.Customers.FirstOrDefault(); // Replace with actual user tracking
            if (user == null)
            {
                TempData["Title"] = "Error";
                TempData["Message"] = "Customer not found. Please log in.";
                TempData["Icon"] = "error";
                return RedirectToAction("Login");
            }

            // Calculate total
            decimal totalAmount = (decimal)CartData.Sum(item => item.product.Srice * item.quantity);

            // Create Order
            var order = new Order
            {
                CustomerFid = user.CustomerId,
                OrderDate = DateTime.Now.Date,
                Time = TimeOnly.FromDateTime(DateTime.Now),
                TotalAmount = totalAmount,
                Status = "Pending"
            };

            // Add order details
            foreach (var item in CartData)
            {
                order.OrderDetails.Add(new OrderDetail
                {
                    ProductFid = item.product.ProductId,
                    Quantity = item.quantity,
                    UnitPrice = item.product.Srice
                });
            }

            // Save to DB
            db.Orders.Add(order);
            db.SaveChanges();

            // Clear Cart
            CartData.Clear();

            TempData["Title"] = "Order Placed";
            TempData["Message"] = "Your order has been placed successfully!";
            TempData["Icon"] = "success";

            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmOrder(string PaymentMethod)
        {

            // Handle payment logic
            if (PaymentMethod == "CashOnDelivery")
            {
                // Handle COD
            }
            else if (PaymentMethod == "CardPayment")
            {
                // Redirect to card payment gateway or simulate
            }
            else if (PaymentMethod == "OnlineTransfer")
            {
                // Handle online transfer details (e.g., show account info)
            }










            if (CartData == null || !CartData.Any())
            {
                TempData["Message"] = "Cart is empty.";
                return RedirectToAction("Cart");
            }

            string email = TempData["Email"]?.ToString();
            if (string.IsNullOrEmpty(email))
            {
                TempData["Message"] = "Customer details missing.";
                return RedirectToAction("Checkout");
            }

            var customer = db.Customers.FirstOrDefault(c => c.Email == email);
            if (customer == null)
            {
                customer = new Customer
                {
                    FullName = TempData["FullName"]?.ToString(),
                    Email = email,
                    Role = "User"
                };
                db.Customers.Add(customer);
                db.SaveChanges();
            }

            var order = new Order
            {
                CustomerFid = customer.CustomerId,
                OrderDate = DateTime.Now,
                Time = TimeOnly.FromDateTime(DateTime.Now),
                TotalAmount = CartData.Sum(i => i.product.Srice * i.quantity),
                Status = "Paid"
            };

            foreach (var item in CartData)
            {
                order.OrderDetails.Add(new OrderDetail
                {
                    ProductFid = item.product.ProductId,
                    Quantity = item.quantity,
                    UnitPrice = item.product.Srice
                });
            }

            db.Orders.Add(order);
            db.SaveChanges();

            CartData.Clear();

            return RedirectToAction("OrderConfirmation");
        }


        public IActionResult OrderConfirmation()
        {
            return View();
        }

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


