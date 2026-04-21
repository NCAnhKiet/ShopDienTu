using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopDienTu.Models.Authentication;
using ShopDienTu.MoDels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace ShopDienTu.Areas.Admin.Controllers
{
    [Area("admin")]
    [AdminAuthentication]
    [Route("admin")]
    [Route("admin/homeadmin")]
    public class HomeAdminController : Controller
    {
        private readonly ShopDienTuContext db;
        private readonly IWebHostEnvironment _env;

        public HomeAdminController(ShopDienTuContext context, IWebHostEnvironment env)
        {
            db = context;
            _env = env;
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

        [Route("")]
        [Route("index")]

        public IActionResult Index()
        {
            var today = DateTime.Now.Date;
            var last30Days = today.AddDays(-30);

            // 1. Tổng doanh thu (All time & Today)
            var allTimeRevenue = db.Orders.Where(o => o.Status == "Hoàn thành").Sum(o => (decimal?)o.TotalPrice) ?? 0;
            var todayRevenue = db.Orders.Where(o => o.Status == "Hoàn thành" && o.OrderDate >= today).Sum(o => (decimal?)o.TotalPrice) ?? 0;

            ViewBag.AllTimeRevenue = allTimeRevenue;
            ViewBag.TodayRevenue = todayRevenue;

            // 2. Đơn hàng (Cần xử lý / Tổng đơn)
            var totalOrders = db.Orders.Count();
            var pendingOrders = db.Orders.Count(o => o.Status == "Đã đặt" || o.Status == "Đang xử lý");
            
            ViewBag.TotalOrders = totalOrders;
            ViewBag.PendingOrders = pendingOrders;

            // 3. Sản phẩm
            ViewBag.TotalProducts = db.Products.Count();

            // 4. Khách hàng
            ViewBag.TotalCustomers = db.Customers.Count();

            // 5. Thống kê Doanh thu 30 ngày gần nhất
            var revenueData = db.Orders
                .Where(o => o.Status == "Hoàn thành" && o.OrderDate >= last30Days)
                .Select(o => new { o.OrderDate.Date, o.TotalPrice })
                .ToList()
                .GroupBy(o => o.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Total = g.Sum(x => x.TotalPrice)
                }).ToList();

            ViewBag.RevenueDates = System.Text.Json.JsonSerializer.Serialize(revenueData.Select(x => x.Date));
            ViewBag.RevenueTotals = System.Text.Json.JsonSerializer.Serialize(revenueData.Select(x => x.Total));

            // 6. Thống kê Lượng đơn 30 ngày gần nhất
            var orderData = db.Orders
                .Where(o => o.OrderDate >= last30Days)
                .Select(o => o.OrderDate.Date)
                .ToList()
                .GroupBy(d => d)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Count = g.Count()
                }).ToList();

            ViewBag.OrderDates = System.Text.Json.JsonSerializer.Serialize(orderData.Select(x => x.Date));
            ViewBag.OrderCounts = System.Text.Json.JsonSerializer.Serialize(orderData.Select(x => x.Count));

            return View();
        }

        [Route("danhmucsanpham")]
        public IActionResult DanhMucSanPham(int page = 1)
        {
            int pageSize = 10;
            var totalItems = db.Products.Count();
            var activeCategories = db.Categories.Count();
            var stockValue = db.Products.Sum(p => (decimal?)p.Price) ?? 0;

            ViewBag.ActiveCategories = activeCategories;
            // Do Entity Product hiện tại chưa có trường Số Lượng (Stock/Quantity), 
            // tạm thời gán bằng 0 hoặc bạn có thể thay đổi bằng logic khác phù hợp
            ViewBag.LowStockAlerts = 0;
            ViewBag.StockValue = stockValue;
            ViewBag.TotalItemsCount = totalItems;
            
            // Do Product chưa lưu ngày tạo (CreatedAt), 
            // tạm thời mock tỉ lệ tăng trưởng để giao diện nhận dữ liệu động
            ViewBag.ProductGrowth = "+12%"; 

            var lstSanPham = db.Products.OrderByDescending(x => x.ProductId)
                                        .Skip((page - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            return View(lstSanPham);
        }

        [Route("ThemSanPhamMoi")]
        [HttpGet]
        public IActionResult ThemSanPhamMoi()
        {
            ViewBag.CategoryId = new SelectList(db.Categories.ToList(),"CategoryId","CategoryName");
            return View();
        }
        [Route("ThemSanPhamMoi")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemSanPhamMoi(Product sanPham, IFormFile? ImageUpload)
        {
            if (ModelState.IsValid)
            {
                if (ImageUpload != null && ImageUpload.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "ProductsImages");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString("N") + Path.GetExtension(ImageUpload.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        ImageUpload.CopyTo(fileStream);
                    }
                    // Chỉ lưu tên file, PartialSanPham.cshtml sẽ tự ghép ~/ProductsImages/
                    sanPham.ImageUrl = uniqueFileName;
                }
                else
                {
                    sanPham.ImageUrl = "placeholder.jpg";
                }

                db.Products.Add(sanPham);
                db.SaveChanges();
                LogActivity("Thêm sản phẩm", $"Thêm mới: {sanPham.ProductName}");
                return RedirectToAction("DanhMucSanPham");
            }

            ViewBag.CategoryId = new SelectList(db.Categories.ToList(), "CategoryId", "CategoryName");
            return View(sanPham);
        }

        [HttpPost]
        [Route("ThemCategoryAjax")]
        public IActionResult ThemCategoryAjax([FromBody] Category cat)
        {
            if (string.IsNullOrWhiteSpace(cat.CategoryName))
                return BadRequest();

            if (string.IsNullOrWhiteSpace(cat.Icon))
                cat.Icon = "bi-grid"; // default icon

            db.Categories.Add(cat);
            db.SaveChanges();
            
            return Json(new { success = true, categoryId = cat.CategoryId, categoryName = cat.CategoryName });
        }

        [HttpPost]
        [Route("XoaCategoryAjax")]
        public IActionResult XoaCategoryAjax(int id)
        {
            var cat = db.Categories.FirstOrDefault(c => c.CategoryId == id);
            if (cat == null)
                return Json(new { success = false, message = "Danh mục không tồn tại!" });

            // Nếu đã có sản phẩm dùng danh mục, báo lỗi
            if (db.Products.Any(p => p.CategoryId == id))
            {
                return Json(new { success = false, message = "Không thể xóa! Đã có sản phẩm thuộc danh mục này." });
            }

            db.Categories.Remove(cat);
            db.SaveChanges();
            LogActivity("Xóa danh mục", $"Xóa danh mục ID: {id}");

            return Json(new { success = true });
        }

        [Route("SuaSanPham")]
        [HttpGet]
        public IActionResult SuaSanPham(int productId)
        {
            ViewBag.CategoryId = new SelectList(db.Categories.ToList(), "CategoryId", "CategoryName");
            var sanPham = db.Products.Find(productId);
            return View(sanPham);
        }
        [Route("SuaSanPham")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SuaSanPham(Product sanPham, IFormFile? ImageUpload)
        {
            if (ModelState.IsValid)
            {
                if (ImageUpload != null && ImageUpload.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "ProductsImages");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString("N") + Path.GetExtension(ImageUpload.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        ImageUpload.CopyTo(fileStream);
                    }
                    // Chỉ lưu tên file, PartialSanPham.cshtml sẽ tự ghép ~/ProductsImages/
                    sanPham.ImageUrl = uniqueFileName;
                }
                // Nếu không upload ảnh mới: giữ nguyên ImageUrl cũ (được truyền qua hidden field)

                db.Update(sanPham);
                db.SaveChanges();
                LogActivity("Sửa sản phẩm", $"Cập nhật ID: {sanPham.ProductId}");
                return RedirectToAction("DanhMucSanPham", "HomeAdmin");
            }
            ViewBag.CategoryId = new SelectList(db.Categories.ToList(), "CategoryId", "CategoryName");
            return View(sanPham);
        }

        [Route("XoaSanPham")]
        [HttpPost]
        public IActionResult XoaSanPham(int productId)
        {
            //kiểm tra sản phẩm có nằm trong giỏ hàng hay chưa
            TempData["Message"] = "";
            var chiTietSanPhams = db.CartDetails.Where(x=>x.ProductId==productId).ToList();
            if (chiTietSanPhams.Count() > 0)
            {
                TempData["Message"] = "Sản phẩm này không xóa được";
                return RedirectToAction("DanhMucSanPham", "HomeAdmin");
            }

            // Lấy sản phẩm
            var sanPham = db.Products.FirstOrDefault(x => x.ProductId == productId);

            if (sanPham == null)
            {
                TempData["Message"] = "Sản phẩm không tồn tại!";
                return RedirectToAction("DanhMucSanPham", "HomeAdmin");
            }

            // Xóa sản phẩm
            db.Products.Remove(sanPham);
            db.SaveChanges();
            LogActivity("Xóa sản phẩm", $"Đã xóa sản phẩm ID: {productId}");

            TempData["Message"] = "Xóa sản phẩm thành công!";
            return RedirectToAction("DanhMucSanPham", "HomeAdmin");
        }

        [Route("orders")]
        public IActionResult Orders(int page = 1)
        {
            int pageSize = 10;
            var query = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate);

            var totalItems = query.Count();
            
            ViewBag.TotalOrdersCount = totalItems;
            ViewBag.PendingOrders = query.Count(o => o.Status == "Đã đặt" || o.Status == "Đang xử lý");
            var totalCompleted = query.Count(o => o.Status == "Hoàn thành");
            var successRate = totalItems == 0 ? 0 : Math.Round((double)totalCompleted / totalItems * 100, 1);
            ViewBag.SuccessRate = successRate.ToString("0.0");

            var orders = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            return View(orders);
        }

[HttpPost]
[Route("Orders/ChangeStatus")]
public IActionResult ChangeStatus(int orderId, string status)
{
    var order = db.Orders.FirstOrDefault(o => o.OrderId == orderId);
    if (order == null)
        return RedirectToAction("Orders");

    status = status?.Trim();

    // ✅ Danh sách trạng thái hợp lệ
    string[] allow =
    {
        "Đã đặt",
        "Đang xử lý",
        "Đang giao",
        "Hoàn thành",
        "Đã hủy"
    };

    // ❌ Không cho set trạng thái rác
    if (!allow.Contains(status))
        return RedirectToAction("Orders");

    // ❌ Không cho đổi nếu đã hoàn thành hoặc đã hủy
    if (order.Status == "Hoàn thành" || order.Status == "Đã hủy")
        return RedirectToAction("Orders");

    // ❌ Không cho quay ngược trạng thái
    int oldIndex = Array.IndexOf(allow, order.Status);
    int newIndex = Array.IndexOf(allow, status);

    if (newIndex < oldIndex && status != "Đã hủy")
        return RedirectToAction("Orders");

    // ❌ Không cho hủy khi đang giao
    if (order.Status == "Đang giao" && status == "Đã hủy")
        return RedirectToAction("Orders");

    order.Status = status;
    db.SaveChanges();
    LogActivity("Cập nhật đơn hàng", $"Đổi trạng thái đơn #{orderId} -> {status}");

    return RedirectToAction("Orders");
}



        [HttpPost]
        [Route("Orders/Cancel")]
        public IActionResult CancelOrder(int orderId)
        {
            var order = db.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
                return RedirectToAction("Orders");

            // ❌ Không cho hủy nếu đang giao hoặc đã hoàn thành
            if (order.Status == "Đang giao" || order.Status == "Hoàn thành")
                return RedirectToAction("Orders");

            order.Status = "Đã hủy";
            db.SaveChanges();
            LogActivity("Hủy đơn", $"Đã hủy đơn hàng #{orderId}");

            return RedirectToAction("Orders");
        }


    }
}
