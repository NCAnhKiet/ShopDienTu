using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDienTu.MoDels;

namespace ShopDienTu.Controllers
{
    public class CartController : Controller
    {
        private readonly ShopDienTuContext db;

        public CartController(ShopDienTuContext context)
        {
            db = context;
        }

        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            ViewBag.SidebarCategories = db.Categories.OrderBy(c => c.CategoryName).ToList();
            base.OnActionExecuting(context);
        }

        //  XEM GIỎ HÀNG
        public IActionResult Index()
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");

            if (customerId == null)
                return RedirectToAction("Login", "Access");

            var cart = db.Carts.FirstOrDefault(c => c.CustomerId == customerId);

            if (cart == null)
            {
                ViewBag.Empty = true;
                return View(new List<CartDetail>()); // TRẢ LIST RỖNG
            }

            // Include(Product)
            var items = db.CartDetails
                          .Where(cd => cd.CartId == cart.CartId)
                          .Include(cd => cd.Product)
                          .ToList();

            return View(items);
        }

        //  THÊM VÀO GIỎ HÀNG
        [HttpPost]
        public IActionResult Add(int id, int quantity)
        {
            if (quantity < 1) quantity = 1;

            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
                return RedirectToAction("Login", "Access");

            var cart = db.Carts.FirstOrDefault(c => c.CustomerId == customerId);

            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerId = customerId,
                    CreatedDate = DateTime.Now
                };
                db.Carts.Add(cart);
                db.SaveChanges();
            }

            var detail = db.CartDetails
                .FirstOrDefault(cd => cd.CartId == cart.CartId && cd.ProductId == id);

            if (detail == null)
            {
                detail = new CartDetail
                {
                    CartId = cart.CartId,
                    ProductId = id,
                    Quantity = quantity
                };
                db.CartDetails.Add(detail);
            }
            else
            {
                detail.Quantity += quantity;  // cộng đúng số lượng bạn chọn
            }

            db.SaveChanges();

            return RedirectToAction("Index");
        }



        //  XÓA SẢN PHẨM
        [HttpPost]
        public IActionResult Remove(int id)
        {
            var item = db.CartDetails.FirstOrDefault(x => x.CartDetailId == id);
            if (item != null)
            {
                db.CartDetails.Remove(item);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        //  UPDATE SỐ LƯỢNG
        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var item = db.CartDetails.FirstOrDefault(x => x.CartDetailId == id);

            if (item != null)
            {
                item.Quantity = quantity;
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Test()
        {
            return Content("CustomerId = " + HttpContext.Session.GetInt32("CustomerId"));
        }

        [HttpGet]
        public IActionResult GetCount()
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null) return Json(0);

            var totalQuantity = db.CartDetails
                .Where(cd => cd.Cart.CustomerId == customerId)
                .Sum(cd => cd.Quantity);

            return Json(totalQuantity);
        }

    }
}
