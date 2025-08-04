using e_shop.Models;
using e_shop.Models.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using StripeCustomer = Stripe.Customer;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;


namespace e_shop.Controllers
{
    public class HomeController : Controller
    {
        public static List<CartItem> CartData = new List<CartItem>();

        private readonly ILogger<HomeController> _logger;
        private Customerwebsite1Context db;
        private Customerwebsite1Context _context;
        private readonly IConfiguration _configuration;
      
        public HomeController(ILogger<HomeController> logger, Customerwebsite1Context context, IConfiguration configuration)
        {
            _logger = logger;
            db = new Customerwebsite1Context();
            _context = context;
            _configuration = configuration;

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


        //for register
        [AllowAnonymous]
        public IActionResult Register(string txtName, string txtEmail, string txtPass, string txtCPass, bool acceptTerms)
        {
            if (acceptTerms == true)
            {
                TempData["Title"] = "Error";
                TempData["Message"] = "You must accept the terms and conditions to register.";
                TempData["Icon"] = "warning";
                return Redirect("/Home/Login");
            }

            if (string.IsNullOrWhiteSpace(txtPass))
            {
                TempData["Title"] = "Error";
                TempData["Message"] = "Password cannot be empty.";
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

            var newUser = new Models.Customer
            {
                FullName = txtName,
                Email = txtEmail,
                Password = BCrypt.Net.BCrypt.HashPassword(txtPass),
                Role = "User"
            };

            db.Customers.Add(newUser);
            db.SaveChanges();

            TempData["Title"] = "Account Registered";
            TempData["Message"] = "Account has been created successfully";
            TempData["Icon"] = "success";

            return Redirect("/Home/Login");
        }


        //LOGIN GET
       
        [Authorize]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }
      
        //LOGIN POST
    
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> LoginCheck(string txtEmail, string txtPass)
        {
            txtEmail = txtEmail?.Trim();

            // Try Admin Login
            var admin = db.Admins.FirstOrDefault(x => x.Email == txtEmail);
            if (admin != null && BCrypt.Net.BCrypt.Verify(txtPass, admin.Passwor)) // Typo fixed: Passwor -> Password
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, admin.Username ?? ""),
            new Claim(ClaimTypes.Email, admin.Email ?? ""),
            new Claim(ClaimTypes.Role, "Admin")
        };

                var identity = new ClaimsIdentity(claims, "MyCookieAuth");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("MyCookieAuth", principal);

                TempData["Message"] = "Admin login successful";
                return RedirectToAction("Admin", "Home");
            }

            // Try Customer Login
            var user = db.Customers.FirstOrDefault(x => x.Email == txtEmail);
            if (user != null && BCrypt.Net.BCrypt.Verify(txtPass, user.Password))
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.FullName ?? ""),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Role, user.Role ?? "User"),
            new Claim("ProfileImage", user.Image ?? "") // ✅ Profile image for navbar
        };

                var identity = new ClaimsIdentity(claims, "MyCookieAuth");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("MyCookieAuth", principal);

                TempData["Message"] = "User login successful";
                return RedirectToAction("Index", "Home");
            }

            // Login Failed
            TempData["Title"] = "Error";
            TempData["Message"] = "Email or Password is incorrect";
            TempData["Icon"] = "error";
            return RedirectToAction("Login");
        }

        //LOGOUT
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Login", "Home");
        }

    
        //USER PROFILE GET
    
        [HttpGet]
        [Authorize(Roles = "User")]
        public IActionResult UserProfile()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var user = db.Customers.FirstOrDefault(x => x.Email == email);
            if (user == null)
                return NotFound();

            var model = new UserProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                ImagePath = user.Image
            };

            return View(model);
        }
        
        //USER PROFILE POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UserProfile(UserProfileViewModel model)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var user = db.Customers.FirstOrDefault(x => x.Email == email);
            if (user == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                // Update user fields
                user.FullName = model.FullName;
                user.Phone = model.Phone;
                user.Address = model.Address;

                // Image upload (optional)
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    try
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Users");
                        Directory.CreateDirectory(uploadPath);

                        var fullPath = Path.Combine(uploadPath, fileName);
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            model.ImageFile.CopyTo(stream);
                        }

                        user.Image = "/Images/Users/" + fileName;
                    }
                    catch
                    {
                        ModelState.AddModelError("ImageFile", "Image upload failed.");
                        model.ImagePath = user.Image;
                        return View(model);
                    }
                }

                // Password update (optional)
                if (!string.IsNullOrWhiteSpace(model.CurrentPassword) && !string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password))
                    {
                        ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                        model.ImagePath = user.Image;
                        return View(model);
                    }

                    if (model.NewPassword != model.ConfirmPassword)
                    {
                        ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                        model.ImagePath = user.Image;
                        return View(model);
                    }

                    user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                }

                // Save changes
                db.Entry(user).State = EntityState.Modified;
                var affected = db.SaveChanges();

                // ✅ Update authentication claims so nav info reflects new data
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.CustomerId.ToString()),
            new(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("ProfileImage", user.Image ?? "/Images/default.png"),
            new Claim(ClaimTypes.Role, "User")
        };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync("MyCookieAuth", principal);


                TempData["Message"] = affected > 0 ? "Profile updated successfully." : "No changes detected.";
                return RedirectToAction("UserProfile");
            }

            model.ImagePath = user.Image;
            return View(model);
        }
        //HASHING PREVIOUS USER PASSWORDS
        //public IActionResult HashUserPasswords()
        //{
        //    var users = db.Customers.ToList();
        //    foreach (var user in users)
        //    {
        //        if (!user.Password.StartsWith("$2"))
        //        {
        //            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
        //        }
        //    }

        //    db.SaveChanges();
        //    return Content("User passwords hashed successfully.");
        //}
















        //ADMIN DASHBOARD


        public IActionResult Admin()
        {
            // show something here, maybe dashboard or admin panel
            return View();
        }

        //ADMIN PROFILE GET

        [Authorize(Roles = "Admin")]
        public IActionResult Profile()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var admin = db.Admins.FirstOrDefault(a => a.Email == email);
            if (admin == null)
                return RedirectToAction("Login");

            var model = new AdminProfileViewModel
            {
                Username = admin.Username,
                Email = admin.Email
            };

            return View(model);
        }

        //ADMIN PROFILE POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Profile(AdminProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var admin = db.Admins.FirstOrDefault(a => a.Email == email);
            if (admin == null)
                return RedirectToAction("Login");

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, admin.Passwor))
            {
                ModelState.AddModelError("CurrentPassword", "Incorrect current password.");
                return View(model);
            }

            // Update fields
            admin.Username = model.Username;
            admin.Email = model.Email;

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                admin.Passwor = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            }

            db.SaveChanges();

            // 🔁 Update claims and re-sign in
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, admin.Username),
        new Claim(ClaimTypes.Email, admin.Email),
        new Claim(ClaimTypes.Role, "Admin")
    };

            var identity = new ClaimsIdentity(claims, "MyCookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("MyCookieAuth", principal);

            TempData["Message"] = "Profile updated successfully.";
            return RedirectToAction("Profile");
        }
      
        
        //TO ADD NEW ADMIN
     
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult AddAdmin(AddAdminViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check if email already exists
            if (db.Admins.Any(a => a.Email == model.Email))
            {
                ModelState.AddModelError("Email", "An admin with this email already exists.");
                return View(model);
            }

            var newAdmin = new Admin
            {
                Username = model.Username,
                Email = model.Email,
                Passwor = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "Admin",  // Assuming your Admin model has a Role field
                Status = "Active"  // Optional: Set a default status
            };

            db.Admins.Add(newAdmin);
            db.SaveChanges();

            TempData["Message"] = "New admin added successfully.";
            return RedirectToAction("Admin"); // or wherever you list admins
        }

        //TO ADD NEW ADMIN

        [Authorize(Roles = "Admin")]
        public IActionResult AddAdmin()
        {
            return View();
        }

        //TO SHOW ALL ADMINS

        [Authorize(Roles = "Admin")]
        public IActionResult AllAdmins()
        {
            var admins = db.Admins.ToList();
            return View(admins);
        }

        //HASHING PREVIOUS ADMIN PASSWORDS

        //public IActionResult HashAdminPasswords()
        //{
        //    var admins = db.Admins.ToList();

        //    foreach (var admin in admins)
        //    {
        //        if (!admin.Passwor.StartsWith("$2")) // not already hashed
        //        {
        //            admin.Passwor = BCrypt.Net.BCrypt.HashPassword(admin.Passwor);
        //        }
        //    }

        //    db.SaveChanges();
        //    return Content("Hashed all plain-text admin passwords.");
        //}

        
        
        //PRODUCTS VIEW
        public IActionResult Products(int? id, int? brand)
        {
            //Displaying brands and categories on side
            ViewBag.Cats = db.Categories.ToList();
            //ViewBag.Brands =db.Brands.ToList();


            //filter category
            var Products = db.Products.Include(x => x.CategoryF).ToList();
            if (id != null)
            {
                return View(Products.Where(x => x.CategoryFid == id));
            }

            //filter brand
            //if (brand != null)
            //{
            //    return View(Products.Where(x => x.BrandFid == brand));
            //}

            return View(Products);

        }

        //DETAILS OF PRODUCT
        public IActionResult Details(int id)
        {
            var product = db.Products.Include(x => x.CategoryF).FirstOrDefault(x => x.ProductId == id);
            return View(product);
        }


        //CATEGORIES VIEW
        public IActionResult Categories()
        {

            var Categories = db.Categories.ToList();
            return View(Categories);
        }

        //BRANDS VIEW
        //public IActionResult Brands()
        //{

        //    var brands = db.Brands.ToList();
        //    return View(brands);
        //}


        //CART VIEW
        public IActionResult Cart()
        {
            return View(CartData);
        }


        //ADDING TO CART
        public IActionResult AddToCart(int id)
        {

            var p = db.Products.Include(x => x.CategoryF).FirstOrDefault(x => x.ProductId == id);

            if (CheckForExistingItem(id) == -1)
            {
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


        // CHECKOUT GET
        [Authorize(AuthenticationSchemes = "MyCookieAuth", Roles = "User")]
        public IActionResult Checkout()
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            CheckoutViewModel model = new CheckoutViewModel
            {
                CartItems = HomeController.CartData ?? new List<CartItem>()
            };

            if (!string.IsNullOrEmpty(userEmail))
            {
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

        // CHECKOUT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = "MyCookieAuth", Roles = "User")]
        public IActionResult Checkout(CheckoutViewModel model)
        {
            if (HomeController.CartData == null || !HomeController.CartData.Any())
            {
                ModelState.AddModelError("", "Your cart is empty. Please add items before proceeding to payment.");
                model.CartItems = new List<CartItem>();
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                model.CartItems = HomeController.CartData;
                return View(model);
            }

            TempData["FullName"] = model.FullName;
            TempData["Email"] = model.Email;
            TempData["Phone"] = model.Phone;
            TempData["Address"] = model.Address;

            return RedirectToAction("Payment");
        }

        // PAYMENT
        public IActionResult Payment()
        {
            TempData.Keep(); // 🔁 Keep TempData for next steps
            ViewBag.TotalAmount = CartData.Sum(i => (i.product.Srice ?? 0) * i.quantity);
            return View();
        }

        // CONFIRM ORDER
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmOrder(string PaymentMethod)
        {
            TempData["PaymentMethod"] = PaymentMethod;
            TempData.Keep(); // Keep all TempData keys

            if (PaymentMethod == "CashOnDelivery")
            {
                return RedirectToAction("PlaceOrderCOD");
            }
            else if (PaymentMethod == "OnlineTransfer")
            {
                // You can redirect to bank details or manual confirmation
                return RedirectToAction("BankTransferInstructions");
            }
            else if (PaymentMethod == "Stripe")
            {
                return RedirectToAction("StripePayment");
            }



            return RedirectToAction("Cart");
        }

        // PLACE ORDER - COD
        public IActionResult PlaceOrderCOD()
        {
            if (!CartData.Any())
            {
                TempData["Message"] = "Cart is empty.";
                return RedirectToAction("Cart");
            }

            string? email = TempData.Peek("Email")?.ToString();
            if (string.IsNullOrEmpty(email))
            {
                TempData["Message"] = "Customer not found.";
                return RedirectToAction("Login");
            }

            var customer = db.Customers.FirstOrDefault(c => c.Email == email);
            if (customer == null)
            {
                customer = new e_shop.Models.Customer
                {
                    FullName = TempData.Peek("FullName")?.ToString(),
                    Email = email,
                    Role = "User"
                };

                db.Customers.Add(customer);
                db.SaveChanges();
            }
            else
            {
                string? fullName = TempData.Peek("FullName")?.ToString();
                if (!string.IsNullOrWhiteSpace(fullName) && fullName != customer.FullName)
                {
                    customer.FullName = fullName;
                    db.SaveChanges();
                }
            }

            decimal totalAmount = CartData.Sum(item => (item.product.Srice ?? 0) * item.quantity);

            var order = new e_shop.Models.Order
            {
                CustomerFid = customer.CustomerId,
                OrderDate = DateTime.Now.Date,
                Time = TimeOnly.FromDateTime(DateTime.Now),
                TotalAmount = totalAmount,
                Status = "Pending",
                PaymentMethod = "CashOnDelivery"
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

            TempData["Message"] = "Your COD order has been placed!";
            return RedirectToAction("OrderConfirmation");
        }

        // STRIPE PAYMENT GET
        public IActionResult StripePayment()
        {
            decimal total = (decimal)CartData.Sum(i => i.product.Srice * i.quantity);
            long amountInPaise = (long)(total * 100); // Stripe uses smallest currency unit

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInPaise,
                Currency = "inr",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            };

            var service = new PaymentIntentService();
            var paymentIntent = service.Create(options);

            ViewBag.ClientSecret = paymentIntent.ClientSecret;
            ViewBag.Amount = total;
            return View();
        }




        // CONFIRM STRIPE PAYMENT
        public IActionResult ConfirmStripePayment(string paymentIntentId)
        {
            var service = new PaymentIntentService();
            var intent = service.Get(paymentIntentId);

            if (intent.Status != "succeeded")
            {
                TempData["Message"] = "Payment not successful.";
                return RedirectToAction("Cart");
            }

            // Get customer data
            string email = TempData["Email"]?.ToString();
            if (string.IsNullOrEmpty(email))
            {
                TempData["Message"] = "Customer email not found.";
                return RedirectToAction("Login");
            }

            var customer = db.Customers.FirstOrDefault(c => c.Email == email);

            if (customer == null)
            {
                customer = new e_shop.Models.Customer
                {
                    FullName = TempData["FullName"]?.ToString(),
                    Email = email,
                    Role = "User"
                };

                db.Customers.Add(customer);
                db.SaveChanges();
            }

            // Create order
            var order = new Order
            {
                CustomerFid = customer.CustomerId,
                OrderDate = DateTime.Now,
                Time = TimeOnly.FromDateTime(DateTime.Now),
                TotalAmount = CartData.Sum(i => i.product.Srice * i.quantity),
                Status = "Paid",
                PaymentMethod = "Stripe"
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

            TempData["Message"] = "Your order has been placed via Stripe!";
            return RedirectToAction("OrderConfirmation");
        }



        // ORDER CONFIRMATION
        public IActionResult OrderConfirmation()
        {
            return View();
        }


        //MY ORDERS VIEW

        [Authorize(Roles = "User")]
        public IActionResult MyOrders()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var customer = db.Customers.FirstOrDefault(c => c.Email == email);

            if (customer == null)
            {
                TempData["Message"] = "Customer not found.";
                return RedirectToAction("Login");
            }

            var orders = db.Orders
                .Where(o => o.CustomerFid == customer.CustomerId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderDate,
                    o.Time,
                    o.TotalAmount,
                    o.Status,
                    Items = o.OrderDetails.Select(od => new
                    {
                        od.ProductF.ProductName,
                        od.Quantity,
                        od.UnitPrice
                    }).ToList()
                }).ToList();

            return View(orders);
        }



        //CONTACT US PAGE GET
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }


        //CONTACT US PAGE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var message = new ContactMessage
            {
                Name = model.Name,
                Email = model.Email,
                Subject = model.Subject,
                Message = model.Message
            };

            _context.ContactMessages.Add(message);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Your message has been submitted!";
            return RedirectToAction("Contact");
        }

        //CONTACT MESSAGES VIEW FOR ADMIN

        [Authorize(Roles = "Admin")]
        public IActionResult ContactMessages()
        {
            var messages = _context.ContactMessages.OrderByDescending(m => m.SentAt).ToList();
            return View(messages);
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


