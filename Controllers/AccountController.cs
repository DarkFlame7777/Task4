using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Task4.Data;
using Task4.Enums;
using Task4.Models;
using Task4.Services;
using Task4.ViewModels;

namespace Task4.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AccountController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Users");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email && 
                                                                u.PasswordHash == HashPassword(model.Password));
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }
            
            if (user.Status == UserStatus.Blocked)
            {
                ModelState.AddModelError("", "Account is blocked.");
                return View(model);
            }
            
            user.LastLoginTime = DateTime.UtcNow;
            user.LastActivityTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim("Status", user.Status.ToString())
            };
            
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var properties = new AuthenticationProperties { IsPersistent = model.RememberMe };
            
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);
            return RedirectToAction("Index", "Users");
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            
            try
            {
                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    PasswordHash = HashPassword(model.Password),
                    Status = UserStatus.Unverified,
                    IsEmailVerified = false,
                    RegistrationTime = DateTime.UtcNow
                };
                
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                
                var token = GenerateVerificationToken(user.Email);
                _ = Task.Run(() => _emailService.SendVerificationEmailAsync(user.Email, user.Name, token));
                
                TempData["SuccessMessage"] = "Registration successful! Check your email.";
                return RedirectToAction("Login");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Invalid verification token.";
                return RedirectToAction("Login");
            }
            
            try
            {
                var email = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Login");
                }
                
                if (user.Status == UserStatus.Unverified)
                {
                    user.Status = UserStatus.Active;
                    user.IsEmailVerified = true;
                    await _context.SaveChangesAsync();
                }
                
                TempData["SuccessMessage"] = "Email verified successfully! You can now login.";
                return RedirectToAction("Login");
            }
            catch (FormatException)
            {
                TempData["ErrorMessage"] = "Invalid token format.";
                return RedirectToAction("Login");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Verification failed. Please try again.";
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private string GenerateVerificationToken(string email) 
            => Convert.ToBase64String(Encoding.UTF8.GetBytes(email));
    }
}
