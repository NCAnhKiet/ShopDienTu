using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShopDienTu.MoDels;

public partial class Product
{
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
    [StringLength(200, ErrorMessage = "Tên sản phẩm tối đa 200 ký tự")]
    public string ProductName { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng nhập giá")]
    [Range(1, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
    public decimal? Price { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập màu")]
    public string? Color { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập size")]
    public string? Size { get; set; }

    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn danh mục")]
    public int? CategoryId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mô tả")]
    public string? Description { get; set; }

    public virtual ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual Category? Category { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int SoldCount { get; set; }
}
