using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDienTu.Models;
using ShopDienTu.MoDels;

namespace ShopDienTu.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ShopDienTuContext db;

        public CheckoutController(ShopDienTuContext context)
        {
            db = context;
        }

        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            ViewBag.SidebarCategories = db.Categories.OrderBy(c => c.CategoryName).ToList();
            base.OnActionExecuting(context);
        }

        // =============================
        // HIỂN THỊ TRANG CHECKOUT
        // =============================
        public IActionResult Index()
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
                return RedirectToAction("Login", "Access");

            var customer = db.Customers.FirstOrDefault(c => c.CustomerId == customerId);
            var cart = db.Carts.FirstOrDefault(c => c.CustomerId == customerId);

            if (cart == null)
                return RedirectToAction("Index", "Cart");

            var items = db.CartDetails
                          .Include(x => x.Product)
                          .Where(x => x.CartId == cart.CartId)
                          .ToList();

            ViewBag.Customer = customer;

            return View(items);
        }

        // =============================
        // XÁC NHẬN THANH TOÁN
        // =============================
        [HttpPost]
        public IActionResult Confirm(string ReceiverName, string Phone, string Address)
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
                return RedirectToAction("Login", "Access");

            var cart = db.Carts.FirstOrDefault(c => c.CustomerId == customerId);
            if (cart == null)
                return RedirectToAction("Index", "Cart");

            var items = db.CartDetails
                          .Include(x => x.Product)
                          .Where(x => x.CartId == cart.CartId)
                          .ToList();

            if (!items.Any())
                return RedirectToAction("Index", "Cart");

            decimal total = items.Sum(x => (x.Product.Price ?? 0) * x.Quantity);

            // ✔ TẠO ORDER
            Order order = new Order
            {
                CustomerId = customerId.Value,
                ReceiverName = ReceiverName,
                Phone = Phone,
                Address = Address,
                TotalPrice = total,
                OrderDate = DateTime.Now,
                Status = "Đã đặt"
            };

            db.Orders.Add(order);
            db.SaveChanges();

            // ✔ ORDER DETAILS
            foreach (var item in items)
            {
                db.OrderDetails.Add(new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId.Value,
                    Quantity = item.Quantity,
                    Price = item.Product.Price ?? 0
                });
            }

            // ✔ CLEAR CART
            db.CartDetails.RemoveRange(items);
            db.Carts.Remove(cart);
            db.SaveChanges();

            return RedirectToAction("Success");
        }

        // =============================
        // TRANG THÀNH CÔNG
        // =============================
        public IActionResult Success()
        {
            return View();
        }
    }
}
