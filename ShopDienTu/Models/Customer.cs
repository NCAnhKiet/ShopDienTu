using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShopDienTu.MoDels;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string? FullName { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public int Role { get; set; }
    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
}
