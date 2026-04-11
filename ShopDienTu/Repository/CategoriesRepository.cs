using ShopDienTu.MoDels;

namespace ShopDienTu.Repository
{
    public class CategoriesRepository : ICategoriesRepository
    {
        private readonly ShopDienTuContext _context;
        public CategoriesRepository(ShopDienTuContext context)
        {
            _context = context;
        }

        public Category Add(Category category)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();
            return category;
        }

        public Category Delete(string categoryId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Category> GetAllCategories()
        {
            return _context.Categories;
        }

        public Category GetCategory(string categoryId)
        {
            return _context.Categories.Find(categoryId);
        }

        public Category Update(Category category)
        {
            _context.Update(category);
            _context.SaveChanges();
            return category;
        }
    }
}
