using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMedia_AspDotNetCore.Models;

namespace SocialMedia_AspDotNetCore.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<User> _userManager;

        public ChatController(ApplicationDbContext dbContext, UserManager<User> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? userId = null)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return RedirectToAction("Login", "Home");
            }

            // Get all users for the user list (excluding current user)
            var users = await _userManager.Users
                .Where(u => u.Id != currentUserId)
                .Select(u => new
                {
                    u.Id,
                    Name = !string.IsNullOrEmpty(u.FirstName) && !string.IsNullOrEmpty(u.LastName)
                        ? $"{u.FirstName} {u.LastName}"
                        : u.Email ?? "Unknown User",
                    u.Email
                })
                .ToListAsync();

            ViewBag.Users = users;
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.SelectedUserId = userId;

            // Get messages based on whether it's global or private chat
            if (string.IsNullOrEmpty(userId))
            {
                // Global messages (no receiver) - ONLY messages where ReceiverId is explicitly NULL
                // This ensures private messages (which have ReceiverId set) never appear in global chat
                // Double check: ReceiverId must be NULL (not empty string, not any value)
                var globalMessages = await _dbContext.Messages
                    .Include(m => m.User)
                    .Where(m => m.ReceiverId == null || m.ReceiverId == string.Empty)
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(50)
                    .Select(m => new
                    {
                        m.Id,
                        m.UserId,
                        UserName = m.User != null 
                            ? (!string.IsNullOrEmpty(m.User.FirstName) && !string.IsNullOrEmpty(m.User.LastName)
                                ? $"{m.User.FirstName} {m.User.LastName}"
                                : m.User.Email ?? "Unknown User")
                            : "Unknown User",
                        m.Content,
                        m.CreatedAt,
                        IsPrivate = false
                    })
                    .OrderBy(m => m.CreatedAt)
                    .ToListAsync();

                ViewBag.Messages = globalMessages;
                ViewBag.ChatType = "global";
            }
            else
            {
                // Private messages between current user and selected user
                // IMPORTANT: Only get messages where ReceiverId is NOT NULL and matches the conversation
                var privateMessages = await _dbContext.Messages
                    .Include(m => m.User)
                    .Include(m => m.Receiver)
                    .Where(m => m.ReceiverId != null && 
                        m.ReceiverId != string.Empty &&
                        ((m.UserId == currentUserId && m.ReceiverId == userId) ||
                         (m.UserId == userId && m.ReceiverId == currentUserId)))
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(100)
                    .Select(m => new
                    {
                        m.Id,
                        m.UserId,
                        UserName = m.User != null 
                            ? (!string.IsNullOrEmpty(m.User.FirstName) && !string.IsNullOrEmpty(m.User.LastName)
                                ? $"{m.User.FirstName} {m.User.LastName}"
                                : m.User.Email ?? "Unknown User")
                            : "Unknown User",
                        ReceiverName = m.Receiver != null
                            ? (!string.IsNullOrEmpty(m.Receiver.FirstName) && !string.IsNullOrEmpty(m.Receiver.LastName)
                                ? $"{m.Receiver.FirstName} {m.Receiver.LastName}"
                                : m.Receiver.Email ?? "Unknown User")
                            : "Unknown User",
                        m.Content,
                        m.CreatedAt,
                        IsPrivate = true
                    })
                    .OrderBy(m => m.CreatedAt)
                    .ToListAsync();

                ViewBag.Messages = privateMessages;
                ViewBag.ChatType = "private";
                ViewBag.SelectedUserName = users.FirstOrDefault(u => u.Id == userId)?.Name ?? "Unknown User";
            }

            return View();
        }
    }
}

