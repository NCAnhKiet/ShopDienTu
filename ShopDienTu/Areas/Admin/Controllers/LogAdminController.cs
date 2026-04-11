using Microsoft.AspNetCore.Mvc;
using ShopDienTu.Models.Authentication;
using ShopDienTu.MoDels;
using System.Linq;
using System;

namespace ShopDienTu.Areas.Admin.Controllers
{
    [Area("admin")]
    [AdminAuthentication]
    [Route("admin/log")]
    public class LogAdminController : Controller
    {
        private readonly ShopDienTuContext db;

        public LogAdminController(ShopDienTuContext context)
        {
            db = context;
        }

        [Route("")]
        [Route("index")]
        public IActionResult Index(int page = 1)
        {
            int pageSize = 15;
            var query = db.ActivityLogs.OrderByDescending(x => x.CreatedAt);
            var totalItems = query.Count();
            var logs = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            return View(logs);
        }
    }
}
