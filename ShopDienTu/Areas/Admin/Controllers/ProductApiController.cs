using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDienTu.MoDels;

namespace ShopDienTu.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/api/products")]
    [ApiController]
    public class ProductApiController : ControllerBase
    {
        private readonly ShopDienTuContext db;

        public ProductApiController(ShopDienTuContext context)
        {
            db = context;
        }

        // GET: admin/api/products
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult GetAll()
        {
            var products = db.Products
                .Include(x => x.Category)
                .Select(x => new
                {
                    x.ProductId,
                    x.ProductName,
                    x.Price,
                    x.Color,
                    x.Size,
                    x.ImageUrl,
                    x.Description,
                    Category = x.Category!.CategoryName
                })
                .ToList();

            return Ok(products);
        }

        // GET: admin/api/products/5
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var p = db.Products
                .Where(x => x.ProductId == id)
                .Select(x => new
                {
                    x.ProductId,
                    x.ProductName,
                    x.Price,
                    x.Color,
                    x.Size,
                    x.ImageUrl,
                    x.Description,
                    x.CategoryId
                })
                .FirstOrDefault();

            if (p == null)
                return NotFound();

            return Ok(p);
        }

        // POST: admin/api/products
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Create([FromBody] Product model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            db.Products.Add(model);
            db.SaveChanges();

            return Ok(new { message = "Thêm thành công", model.ProductId });
        }

        // PUT: admin/api/products/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Product model)
        {
            if (id != model.ProductId)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var p = db.Products.FirstOrDefault(x => x.ProductId == id);
            if (p == null)
                return NotFound();

            p.ProductName = model.ProductName;
            p.Price = model.Price;
            p.Color = model.Color;
            p.Size = model.Size;
            p.ImageUrl = model.ImageUrl;
            p.CategoryId = model.CategoryId;
            p.Description = model.Description;

            db.SaveChanges();

            return Ok(new { message = "Cập nhật thành công" });
        }

        // DELETE: admin/api/products/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var p = db.Products.FirstOrDefault(x => x.ProductId == id);
            if (p == null)
                return NotFound();

            var inCart = db.CartDetails.Any(x => x.ProductId == id);
            if (inCart)
                return BadRequest("Sản phẩm đang có trong giỏ hàng");

            db.Products.Remove(p);
            db.SaveChanges();

            return Ok(new { message = "Xóa thành công" });
        }
    }
}
