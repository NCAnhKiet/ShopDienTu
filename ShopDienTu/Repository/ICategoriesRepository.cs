using ShopDienTu.MoDels;

namespace ShopDienTu.Repository
{
    public interface ICategoriesRepository
    {
        Category Add(Category category);

        Category Update(Category category);

        Category Delete(string categoryId);

        Category GetCategory(string categoryId);

        IEnumerable<Category> GetAllCategories();
    }
}
