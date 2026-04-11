using Microsoft.AspNetCore.Mvc;
using ShopDienTu.MoDels;

namespace ShopDienTu.Controllers
{
    public class ProductController : Controller
    {
        private readonly ShopDienTuContext db;

        public ProductController(ShopDienTuContext context)
        {
            db = context;
        }

        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            ViewBag.SidebarCategories = db.Categories.OrderBy(c => c.CategoryName).ToList();
            base.OnActionExecuting(context);
        }
        public IActionResult Index(int page = 1)
        {
            int pageSize = 15;

            // Tính tổng số lượng đặt hàng thành công của từng sản phẩm
            var soldDict = db.OrderDetails
                .GroupBy(d => d.ProductId)
                .Select(g => new { ProductId = g.Key, Count = g.Sum(d => d.Quantity) })
                .ToDictionary(x => x.ProductId, x => x.Count);

            // Nạp sản phẩm, gán mốc số lượng bán, sắp xếp bán chạy nhất lên trên
            var query = db.Products.ToList().Select(p => 
            {
                p.SoldCount = soldDict.ContainsKey(p.ProductId) ? soldDict[p.ProductId] : 0;
                return p;
            }).OrderByDescending(x => x.SoldCount).ThenByDescending(x => x.ProductId).ToList();

            var totalItems = query.Count();
            
            var allProducts = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            return View(allProducts);
        }
    }
}
