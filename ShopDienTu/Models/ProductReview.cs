using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopDienTu.MoDels
{
    [Table("ProductReviews")]
    public class ProductReview
    {
        [Key]
        public int ReviewId { get; set; }

        public int ProductId { get; set; }

        public int CustomerId { get; set; }

        [Required]
        public string Content { get; set; } = null!;

        [Range(1, 5)]
        public int Rating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }
    }
}
