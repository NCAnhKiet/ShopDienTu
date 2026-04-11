using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDienTu.Models;
using ShopDienTu.Models.Authentication;
using ShopDienTu.MoDels;
using System.Diagnostics;

namespace ShopDienTu.Controllers
{
    public class HomeController : Controller
    {
        private readonly ShopDienTuContext db;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, ShopDienTuContext context)
        {
            _logger = logger;
            db = context;
        }

        // Truyền danh mục cho Sidebar vào tất cả views
        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            ViewBag.SidebarCategories = db.Categories.OrderBy(c => c.CategoryName).ToList();
            base.OnActionExecuting(context);
        }

        //[Authentication]
        public IActionResult Index()
        {
            var soldDict = db.OrderDetails
                .GroupBy(d => d.ProductId)
                .Select(g => new { ProductId = g.Key, Count = g.Sum(d => d.Quantity) })
                .ToDictionary(x => x.ProductId, x => x.Count);

            var featuredProducts = db.Products
                .ToList()
                .Select(p => { 
                    p.SoldCount = soldDict.ContainsKey(p.ProductId) ? soldDict[p.ProductId] : 0; 
                    return p; 
                })
                .OrderByDescending(p => p.SoldCount)
                .ThenByDescending(p => p.ProductId)
                .Take(10)
                .ToList();

            ViewBag.Categories = db.Categories.ToList();

            return View(featuredProducts);
        }

        public IActionResult SanPhamTheoLoai(int categoryId)
        {
            var soldDict = db.OrderDetails
                .Where(d => d.Product.CategoryId == categoryId)
                .GroupBy(d => d.ProductId)
                .Select(g => new { ProductId = g.Key, Count = g.Sum(d => d.Quantity) })
                .ToDictionary(x => x.ProductId, x => x.Count);

            List<Product> products = db.Products
                .Where(x => x.CategoryId == categoryId)
                .ToList()
                .Select(p => { 
                    p.SoldCount = soldDict.ContainsKey(p.ProductId) ? soldDict[p.ProductId] : 0; 
                    return p; 
                })
                .OrderByDescending(x => x.SoldCount)
                .ThenBy(x => x.ProductName)
                .ToList();
                
            return View(products);
        }

        public IActionResult ChiTietSanPham(int productId)
        {
            var sanPham = db.Products
                .Include(p => p.ProductReviews)
                .ThenInclude(r => r.Customer)
                .SingleOrDefault(x => x.ProductId == productId);
                
            if (sanPham != null) {
                sanPham.SoldCount = db.OrderDetails.Where(d => d.ProductId == productId).Sum(d => (int?)d.Quantity) ?? 0;
            }

            int currentCustomerId = 0;
            var email = HttpContext.Session.GetString("Email");
            if (email != null) {
                var customer = db.Customers.FirstOrDefault(x => x.Email == email);
                if (customer != null) {
                    currentCustomerId = customer.CustomerId;
                    DateTime sevenDaysAgo = DateTime.Now.AddDays(-7);
                    int boughtCount = db.OrderDetails.Include(od => od.Order)
                        .Where(od => od.ProductId == productId && od.Order.CustomerId == currentCustomerId && od.Order.Status != "Đã hủy" && od.Order.OrderDate >= sevenDaysAgo)
                        .Count();
                    int reviewCount = db.ProductReviews.Count(r => r.ProductId == productId && r.CustomerId == currentCustomerId);
                    ViewBag.CanReviewCount = boughtCount - reviewCount;
                }
            }
            ViewBag.CurrentCustomerId = currentCustomerId;

            return View(sanPham);
        }

        [HttpPost]
        public IActionResult SubmitReview(int productId, string content, int rating)
        {
            var email = HttpContext.Session.GetString("Email");
            if (email == null) return RedirectToAction("Login", "Access");

            var customer = db.Customers.FirstOrDefault(x => x.Email == email);
            if (customer == null) return RedirectToAction("Login", "Access");

            DateTime sevenDaysAgo = DateTime.Now.AddDays(-7);
            int boughtCount = db.OrderDetails.Include(od => od.Order)
                .Where(od => od.ProductId == productId && od.Order.CustomerId == customer.CustomerId && od.Order.Status != "Đã hủy" && od.Order.OrderDate >= sevenDaysAgo)
                .Count();
            int reviewCount = db.ProductReviews.Count(r => r.ProductId == productId && r.CustomerId == customer.CustomerId);

            if (boughtCount > reviewCount && !string.IsNullOrWhiteSpace(content) && rating >= 1 && rating <= 5)
            {
                var review = new ProductReview
                {
                    ProductId = productId,
                    CustomerId = customer.CustomerId,
                    Content = content,
                    Rating = rating,
                    CreatedAt = DateTime.Now
                };
                db.ProductReviews.Add(review);
                db.SaveChanges();
            }

            return RedirectToAction("ChiTietSanPham", new { productId = productId });
        }

        [HttpPost]
        public IActionResult DeleteReview(int reviewId)
        {
            var email = HttpContext.Session.GetString("Email");
            if (email == null) return RedirectToAction("Login", "Access");

            var customer = db.Customers.FirstOrDefault(x => x.Email == email);
            if (customer == null) return RedirectToAction("Login", "Access");

            var review = db.ProductReviews.FirstOrDefault(r => r.ReviewId == reviewId && r.CustomerId == customer.CustomerId);
            if (review != null)
            {
                if ((DateTime.Now - review.CreatedAt).TotalHours <= 8)
                {
                    int pId = review.ProductId;
                    db.ProductReviews.Remove(review);
                    db.SaveChanges();
                    return RedirectToAction("ChiTietSanPham", new { productId = pId });
                }
            }
            return RedirectToAction("Index", "Home");
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
