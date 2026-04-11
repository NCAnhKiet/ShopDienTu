using System;
using System.Collections.Generic;

namespace ShopDienTu.MoDels;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;
    public string? Icon { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

}
