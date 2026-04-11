using System;
using System.ComponentModel.DataAnnotations;

namespace ShopDienTu.MoDels
{
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(150)]
        public string Email { get; set; } = null!;

        [MaxLength(100)]
        public string Action { get; set; } = null!;

        public string? Details { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
