using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDienTu.MoDels;

namespace ShopDienTu.Controllers
{
    public class OrderController : Controller
    {
        private readonly ShopDienTuContext db;

        public OrderController(ShopDienTuContext context)
        {
            db = context;
        }

        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            ViewBag.SidebarCategories = db.Categories.OrderBy(c => c.CategoryName).ToList();
            base.OnActionExecuting(context);
        }

        public IActionResult MyOrders(int page = 1)
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
                return RedirectToAction("Login", "Access");

            int pageSize = 5;
            var query = db.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate);

            var totalItems = query.Count();
            var orders = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(orders);
        }

        [HttpPost]
        public IActionResult Cancel(int orderId)
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
                return RedirectToAction("Login", "Access");

            var order = db.Orders.FirstOrDefault(o => o.OrderId == orderId && o.CustomerId == customerId);
            if (order == null)
                return RedirectToAction("MyOrders");

            if (order.Status == "Đã đặt" || order.Status == "Đang xử lý")
            {
                order.Status = "Đã hủy";
                db.SaveChanges();
            }

            return RedirectToAction("MyOrders");
        }
    }

}
