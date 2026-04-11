using Microsoft.AspNetCore.Mvc;
using ShopDienTu.MoDels;

namespace ShopDienTu.Controllers
{
    public class AccessController : Controller
    {
        private readonly ShopDienTuContext db;

        public AccessController(ShopDienTuContext context)
        {
            db = context;
        }

        // ================== LOGIN ==================
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public IActionResult Login(Customer user)
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                var u = db.Customers
                          .FirstOrDefault(x => x.Email == user.Email);

                if (u != null)
                {
                    bool passwordValid = false;

                    // Thử verify bằng BCrypt trước
                    try
                    {
                        passwordValid = BCrypt.Net.BCrypt.Verify(user.Password, u.Password);
                    }
                    catch
                    {
                        // Nếu password trong DB không phải BCrypt hash → thử so sánh plain text (fallback cho mật khẩu cũ)
                        if (u.Password == user.Password)
                        {
                            passwordValid = true;

                            // Tự động hash lại mật khẩu cũ và cập nhật DB
                            u.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                            db.SaveChanges();
                        }
                    }

                    if (passwordValid)
                    {
                        // LƯU SESSION
                        HttpContext.Session.SetString("Email", u.Email);
                        HttpContext.Session.SetInt32("Role", u.Role);
                        HttpContext.Session.SetInt32("CustomerId", u.CustomerId);
                        HttpContext.Session.SetString("FullName", u.FullName ?? "");

                        // ĐIỀU HƯỚNG TÙY ROLE
                        if (u.Role == 1 || u.Role == 3)  // Admin or Staff
                        {
                            return RedirectToAction("Index", "HomeAdmin", new { area = "Admin" });
                        }
                        else              // Customer
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
            }

            ViewBag.Error = "Sai email hoặc mật khẩu!";
            return View();
        }

        // ================= REGISTER =================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Customer model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var emailExist = db.Customers.FirstOrDefault(x => x.Email == model.Email);
            if (emailExist != null)
            {
                ViewBag.Error = "Email đã tồn tại!";
                return View(model);
            }

            model.Role = 2; // Customer

            // Hash password trước khi lưu DB
            model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

            db.Customers.Add(model);
            db.SaveChanges();

            // QUAN TRỌNG — LƯU CUSTOMERID VỪA INSERT
            HttpContext.Session.SetInt32("CustomerId", model.CustomerId);
            HttpContext.Session.SetString("Email", model.Email);
            HttpContext.Session.SetInt32("Role", model.Role);
            HttpContext.Session.SetString("FullName", model.FullName ?? "");

            return RedirectToAction("Index", "Home");
        }


        // ================= LOGOUT =================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
