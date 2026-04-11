using Microsoft.AspNetCore.Mvc;
using ShopDienTu.MoDels;

namespace ShopDienTu.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ShopDienTuContext db;

        public ProfileController(ShopDienTuContext context)
        {
            db = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var email = HttpContext.Session.GetString("Email");
            if (email == null) return RedirectToAction("Login", "Access");

            var customer = db.Customers.FirstOrDefault(x => x.Email == email);
            if (customer == null) return RedirectToAction("Login", "Access");

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(string FullName, string Phone, string Address, string CurrentPassword, string NewPassword)
        {
            var email = HttpContext.Session.GetString("Email");
            if (email == null) return RedirectToAction("Login", "Access");

            var c = db.Customers.FirstOrDefault(x => x.Email == email);
            if (c != null)
            {
                c.FullName = FullName;
                c.Phone = Phone;
                c.Address = Address;

                if (!string.IsNullOrEmpty(NewPassword))
                {
                    if (string.IsNullOrEmpty(CurrentPassword))
                    {
                        TempData["Error"] = "Vui lòng nhập mật khẩu hiện tại nếu muốn đổi mật khẩu mới!";
                        return RedirectToAction("Index");
                    }

                    // Kiểm tra password cũ
                    bool isOldPasswordValid = false;
                    try
                    {
                        isOldPasswordValid = BCrypt.Net.BCrypt.Verify(CurrentPassword, c.Password);
                    }
                    catch
                    {
                        if (c.Password == CurrentPassword) isOldPasswordValid = true;
                    }

                    if (!isOldPasswordValid)
                    {
                        TempData["Error"] = "Mật khẩu hiện tại không chính xác!";
                        return RedirectToAction("Index");
                    }

                    c.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);
                }

                db.SaveChanges();
                HttpContext.Session.SetString("FullName", c.FullName ?? "");
                TempData["Success"] = "Cập nhật hồ sơ thành công!";
            }
            return RedirectToAction("Index");
        }
    }
}
