using System;
using System.Collections.Generic;

namespace ShopDienTu.MoDels;

public partial class User
{
    public int UserId { get; set; }

    public string? FullName { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int Role { get; set; }

}
