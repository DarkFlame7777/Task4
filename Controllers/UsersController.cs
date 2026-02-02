using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Task4.Data;
using Task4.Enums;
using Task4.ViewModels;

namespace Task4.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }
            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null || user.Status == UserStatus.Blocked)
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }
            user.LastActivityTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            var users = await _context.Users
                .Where(u => !u.IsDeleted)
                .OrderByDescending(u => u.LastLoginTime)
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Status = u.Status,
                    LastLoginTime = u.LastLoginTime,
                    LastActivityTime = u.LastActivityTime,
                    RegistrationTime = u.RegistrationTime
                })
                .ToListAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUsers(string userIds)
        {
            if (string.IsNullOrEmpty(userIds))
            {
                TempData["ErrorMessage"] = "Please select users.";
                return RedirectToAction("Index");
            }
            var ids = userIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(int.Parse)
                            .ToList();
            var users = await _context.Users
                .Where(u => ids.Contains(u.Id))
                .ToListAsync();

            foreach (var user in users)
                user.Status = UserStatus.Blocked;
            
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Blocked {users.Count} user(s).";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockUsers(string userIds)
        {
            if (string.IsNullOrEmpty(userIds))
            {
                TempData["ErrorMessage"] = "Please select users.";
                return RedirectToAction("Index");
            }
            var ids = userIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(int.Parse)
                            .ToList();
            var users = await _context.Users
                .Where(u => ids.Contains(u.Id))
                .ToListAsync();
            foreach (var user in users)
            {
                if (user.Status == UserStatus.Blocked)
                {
                    user.Status = user.IsEmailVerified
                        ? UserStatus.Active
                        : UserStatus.Unverified;
                }
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Unblocked {users.Count} user(s).";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUsers(string userIds)
        {
            if (string.IsNullOrEmpty(userIds))
            {
                TempData["ErrorMessage"] = "Please select users.";
                return RedirectToAction("Index");
            }
            var ids = userIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(int.Parse)
                            .ToList();
            var users = await _context.Users
                                    .Where(u => ids.Contains(u.Id))
                                    .ToListAsync();
            _context.Users.RemoveRange(users);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Deleted {users.Count} user(s).";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUnverifiedUsers()
        {
            var users = await _context.Users
                                    .Where(u => u.Status == UserStatus.Unverified)
                                    .ToListAsync();
            if (users.Any())
            {
                _context.Users.RemoveRange(users);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Deleted {users.Count} unverified user(s).";
            }
            else TempData["InfoMessage"] = "No unverified users.";

            return RedirectToAction("Index");
        }
    }
}