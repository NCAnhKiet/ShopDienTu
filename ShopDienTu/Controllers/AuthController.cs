using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ShopDienTu.Models;
using ShopDienTu.MoDels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ShopDienTuContext _db;

    public AuthController(IConfiguration config, ShopDienTuContext db)
    {
        _config = config;
        _db = db;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        // Tìm user trong DB theo email (Username chính là Email)
        var user = _db.Customers.FirstOrDefault(x => x.Email == model.Username);

        if (user == null)
        {
            return Unauthorized("Sai tài khoản hoặc mật khẩu");
        }

        // Verify password bằng BCrypt
        bool passwordValid = false;
        try
        {
            passwordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
        }
        catch
        {
            // Fallback cho mật khẩu cũ (plain text)
            if (user.Password == model.Password)
            {
                passwordValid = true;

                // Tự động hash lại
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                _db.SaveChanges();
            }
        }

        if (!passwordValid)
        {
            return Unauthorized("Sai tài khoản hoặc mật khẩu");
        }

        // Xác định role
        string roleName = user.Role == 1 ? "Admin" : "Customer";

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, roleName)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(
                Convert.ToDouble(_config["Jwt:DurationInMinutes"])),
            signingCredentials: creds);

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token)
        });
    }
}
