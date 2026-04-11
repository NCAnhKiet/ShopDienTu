using ShopDienTu.MoDels;
using Microsoft.AspNetCore.Mvc;
using ShopDienTu.Repository;
namespace ShopDienTu.ViewComponents
{
    public class CategoriesMenuViewComponent : ViewComponent
    {
        private readonly ICategoriesRepository _category;
        public CategoriesMenuViewComponent(ICategoriesRepository categoriesRepository)
        {
            _category = categoriesRepository;
        }
        public IViewComponentResult Invoke()
        {
            var category = _category.GetAllCategories().OrderBy(x=>x.CategoryName);
            return View(category);
        }
    }
}
