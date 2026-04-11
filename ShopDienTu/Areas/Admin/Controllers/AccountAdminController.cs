using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ShopDienTu.Models.Authentication;
using ShopDienTu.MoDels;
using System.Linq;
using System;

namespace ShopDienTu.Areas.Admin.Controllers
{
    [Area("admin")]
    [AdminAuthentication]
    [Route("admin/account")]
    public class AccountAdminController : Controller
    {
        private readonly ShopDienTuContext db;

        public AccountAdminController(ShopDienTuContext context)
        {
            db = context;
        }

        private void LogActivity(string action, string details = "")
        {
            var email = HttpContext.Session.GetString("Email") ?? "Hệ thống";
            db.ActivityLogs.Add(new ActivityLog
            {
                Email = email,
                Action = action,
                Details = details,
                CreatedAt = DateTime.Now
            });
            db.SaveChanges();
        }

        [Route("customers")]
        public IActionResult Customers(int page = 1)
        {
            int pageSize = 10;
            var query = db.Customers.Where(c => c.Role == 2).OrderByDescending(c => c.CustomerId);
            var totalItems = query.Count();
            
            // Stats for modern UI
            ViewBag.TotalCustomersCount = totalItems;
            ViewBag.NewCustomers = 120; // Mock stat matching UI
            ViewBag.ActiveUsers = 85; // Mock stat
            ViewBag.ConversionRate = "3.2%"; // Mock stat

            var customers = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            return View(customers);
        }

        [Route("staffs")]
        public IActionResult Staffs(int page = 1)
        {
            int pageSize = 10;
            var query = db.Customers.Where(c => c.Role == 1 || c.Role == 3).OrderByDescending(c => c.CustomerId);
            var totalItems = query.Count();
            var staffs = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            return View(staffs);
        }

        [Route("CreateStaff")]
        [HttpGet]
        public IActionResult CreateStaff()
        {
            return View();
        }

        [Route("CreateStaff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateStaff(Customer staff, string RoleType)
        {
            if (ModelState.IsValid)
            {
                if (db.Customers.Any(x => x.Email == staff.Email))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại trong hệ thống!");
                    return View(staff);
                }

                // Hash password protection
                staff.Password = BCrypt.Net.BCrypt.HashPassword(staff.Password);
                
                // Set role depending on Admin choice
                staff.Role = RoleType == "1" ? 1 : 3;

                db.Customers.Add(staff);
                db.SaveChanges();

                LogActivity("Thêm nhân sự", $"Thêm mới {(staff.Role == 1 ? "Admin" : "Nhân viên")} email: {staff.Email}");
                return RedirectToAction("Staffs");
            }
            return View(staff);
        }

        [Route("DeleteStaff")]
        [HttpPost]
        public IActionResult DeleteStaff(int id)
        {
            var adminId = HttpContext.Session.GetInt32("CustomerId");
            if (adminId == id) {
                TempData["Message"] = "Không thể tự xóa tài khoản của chính mình đang đăng nhập!";
                return RedirectToAction("Staffs");
            }
            
            var staff = db.Customers.FirstOrDefault(x => x.CustomerId == id && (x.Role == 1 || x.Role == 3));
            if (staff != null) {
                db.Customers.Remove(staff);
                db.SaveChanges();
                LogActivity("Xóa nhân sự", $"Đã xóa email: {staff.Email}");
            }
            return RedirectToAction("Staffs");
        }

        [Route("CreateCustomer")]
        [HttpGet]
        public IActionResult CreateCustomer()
        {
            return View();
        }

        [Route("CreateCustomer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCustomer(Customer customer)
        {
            if (ModelState.IsValid)
            {
                if (db.Customers.Any(x => x.Email == customer.Email))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại!");
                    return View(customer);
                }

                customer.Password = BCrypt.Net.BCrypt.HashPassword(customer.Password);
                customer.Role = 2; // Customer role

                db.Customers.Add(customer);
                db.SaveChanges();

                LogActivity("Thêm khách hàng", $"Thêm mới: {customer.Email}");
                return RedirectToAction("Customers");
            }
            return View(customer);
        }

        [Route("EditCustomer")]
        [HttpGet]
        public IActionResult EditCustomer(int id)
        {
            var customer = db.Customers.Find(id);
            if (customer == null || customer.Role != 2) return RedirectToAction("Customers");
            return View(customer);
        }

        [Route("EditCustomer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCustomer(Customer customer)
        {
            ModelState.Remove("Password");
            ModelState.Remove("Email");
            
            if (ModelState.IsValid)
            {
                var existing = db.Customers.Find(customer.CustomerId);
                if (existing != null)
                {
                    existing.FullName = customer.FullName;
                    existing.Phone = customer.Phone;
                    existing.Address = customer.Address;
                    
                    if (!string.IsNullOrEmpty(customer.Password)) {
                         existing.Password = BCrypt.Net.BCrypt.HashPassword(customer.Password);
                    }
                    
                    db.SaveChanges();
                    LogActivity("Sửa khách hàng", $"Cập nhật ID: {customer.CustomerId}");
                }
                return RedirectToAction("Customers");
            }
            return View(customer);
        }

        [Route("DeleteCustomer")]
        [HttpPost]
        public IActionResult DeleteCustomer(int id)
        {
            var customer = db.Customers.FirstOrDefault(x => x.CustomerId == id && x.Role == 2);
            if (customer != null) {
                db.Customers.Remove(customer);
                db.SaveChanges();
                LogActivity("Xóa khách hàng", $"Đã xóa email: {customer.Email}");
            }
            return RedirectToAction("Customers");
        }
    }
}
