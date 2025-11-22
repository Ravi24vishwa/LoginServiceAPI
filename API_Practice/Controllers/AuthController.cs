//using API_Practice.Models;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;

//[Route("api/[controller]")]
//[ApiController]
//public class AuthController : ControllerBase
//{
//    private readonly UserManager<ApplicationUser> _userManager;
//    private readonly IConfiguration _config;
//    private readonly SignInManager<ApplicationUser> _signInManager;

//    public AuthController(UserManager<ApplicationUser> userManager,
//                          SignInManager<ApplicationUser> signInManager,
//                          IConfiguration config)
//    {
//        _userManager = userManager;
//        _signInManager = signInManager;
//        _config = config;
//    }

//    // ------------------ REGISTER API ------------------
//    [HttpPost("register")]
//    public async Task<IActionResult> Register(RegisterModel model)
//    {
//        var user = new ApplicationUser
//        {
//            UserName = model.Email,
//            Email = model.Email
//        };

//        var result = await _userManager.CreateAsync(user, model.Password);

//        if (!result.Succeeded)
//            return BadRequest(result.Errors);

//        return Ok(new { message = "User registered successfully!" });
//    }

//    // ------------------ LOGIN API ------------------
//    [HttpPost("login")]
//    public async Task<IActionResult> Login(LoginModel model)
//    {
//        var user = await _userManager.FindByEmailAsync(model.Email);
//        if (user == null) return Unauthorized("User not found");

//        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
//        if (!result.Succeeded) return Unauthorized("Invalid credentials");

//        var token = GenerateToken(user);
//        return Ok(new { token });
//    }

//    // ------------------ JWT GENERATOR ------------------
//    private string GenerateToken(ApplicationUser user)
//    {
//        var tokenHandler = new JwtSecurityTokenHandler();
//        var key = Encoding.UTF8.GetBytes(_config["JWT:Key"]);

//        var claims = new[]
//        {
//            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
//            new Claim(JwtRegisteredClaimNames.Email, user.Email),
//        };

//        var tokenDescriptor = new SecurityTokenDescriptor
//        {
//            Subject = new ClaimsIdentity(claims),
//            Expires = DateTime.UtcNow.AddHours(3),
//            Issuer = _config["JWT:Issuer"],
//            Audience = _config["JWT:Audience"],
//            SigningCredentials = new SigningCredentials(
//                new SymmetricSecurityKey(key),
//                SecurityAlgorithms.HmacSha256Signature)
//        };

//        var token = tokenHandler.CreateToken(tokenDescriptor);

//        return tokenHandler.WriteToken(token);
//    }
//}

//using API_Practice;
//using API_Practice.Models;
//using BCrypt.Net;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;

//[Route("api/[controller]")]
//[ApiController]
//public class AuthController : ControllerBase
//{
//    private readonly AppDbContext _db;
//    private readonly IConfiguration _config;

//    public AuthController(AppDbContext db, IConfiguration config)
//    {
//        _db = db;
//        _config = config;
//    }

//    // REGISTER
//    [HttpPost("register")]
//    public IActionResult Register(RegisterModel model)
//    {
//        if (_db.Users.Any(u => u.Email == model.Email))
//            return BadRequest("Email already exists");

//        var user = new User
//        {
//            Email = model.Email,
//            FirstName = model.FirstName,
//            LastName = model.LastName,
//            MobileNumber = model.MobileNumber,
//            Gender = model.Gender,
//            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash)
//        };

//        _db.Users.Add(user);
//        _db.SaveChanges();

//        return Ok("User registered successfully");
//    }

//    // LOGIN
//    [HttpPost("login")]
//    public IActionResult Login(LoginModel model)
//    {
//        var user = _db.Users.FirstOrDefault(u => u.Email == model.Email);
//        if (user == null) return Unauthorized("User not found");

//        if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
//            return Unauthorized("Invalid password");

//        string token = GenerateToken(user);

//        return Ok(new { token });
//    }

//    // JWT TOKEN CREATOR
//    private string GenerateToken(User user)
//    {
//        var claims = new[]
//        {
//            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
//            new Claim(ClaimTypes.Email, user.Email),
//            new Claim("FullName", user.FirstName + " " + user.LastName)
//        };

//        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
//        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//        var token = new JwtSecurityToken(
//            issuer: _config["JWT:Issuer"],
//            audience: _config["JWT:Audience"],
//            claims: claims,
//            expires: DateTime.UtcNow.AddHours(3),
//            signingCredentials: creds
//        );

//        return new JwtSecurityTokenHandler().WriteToken(token);
//    }
//}


using API_Practice.Models;
using API_Practice.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace API_Practice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwt;

        public AuthController(AppDbContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        // ---------------- REGISTER -----------------
        [HttpPost("register")]
        public IActionResult Register(RegisterModel model)
        {
            if (_db.Users.Any(u => u.Email == model.Email))
                return BadRequest("Email already exists");

            var user = new User
            {
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                MobileNumber = model.MobileNumber,
                Gender = model.Gender,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            return Ok("User registered successfully");
        }

        // ---------------- LOGIN -----------------
        [HttpPost("login")]
        public IActionResult Login(LoginModel model)
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
                return Unauthorized("Invalid credentials");

            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");

            var token = _jwt.GenerateToken(user.Email);

            return Ok(new { Token = token });
        }
    }
}
