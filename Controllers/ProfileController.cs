using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMedia_AspDotNetCore.Models;
using SocialMedia_AspDotNetCore.ViewModels;

namespace SocialMedia_AspDotNetCore.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public ProfileController(UserManager<User> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        // GET: /Profile - Show current user's profile
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var profile = await _dbContext.Profiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

            // Profile should always exist (created during registration), but handle if not
            if (profile == null)
            {
                // Create profile if somehow missing
                profile = new Profile
                {
                    UserId = currentUser.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.Profiles.Add(profile);
                await _dbContext.SaveChangesAsync();
                await _dbContext.Entry(profile).Reference(p => p.User).LoadAsync();
            }

            var vm = new UserProfileViewModel
            {
                ProfileId = profile.Id,
                UserId = profile.UserId,
                Email = profile.User?.Email ?? string.Empty,
                FirstName = profile.User?.FirstName,
                LastName = profile.User?.LastName,
                Birthday = profile.Birthday,
                Gender = profile.Gender,
                PhoneNumber = profile.PhoneNumber,
                Address = profile.Address
            };

            return View(vm);
        }

        // GET: /Profile/Create
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Home");
            }

            // Profile always exists (created during registration), so redirect to Edit
            return RedirectToAction(nameof(Edit));
        }

        // POST: /Profile/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UserProfileViewModel model)
        {
            // Profile always exists, redirect to Edit instead
            return RedirectToAction(nameof(Edit));
        }

        // GET: /Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var profile = await _dbContext.Profiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

            // Profile should always exist, but handle if missing
            if (profile == null)
            {
                profile = new Profile
                {
                    UserId = currentUser.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.Profiles.Add(profile);
                await _dbContext.SaveChangesAsync();
                await _dbContext.Entry(profile).Reference(p => p.User).LoadAsync();
            }

            var vm = new UserProfileViewModel
            {
                ProfileId = profile.Id,
                UserId = profile.UserId,
                Email = profile.User?.Email ?? string.Empty,
                FirstName = profile.User?.FirstName,
                LastName = profile.User?.LastName,
                Birthday = profile.Birthday,
                Gender = profile.Gender,
                PhoneNumber = profile.PhoneNumber,
                Address = profile.Address
            };

            return View(vm);
        }

        // POST: /Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserProfileViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Home");
            }

            if (!ModelState.IsValid)
            {
                model.Email = currentUser.Email ?? string.Empty;
                model.FirstName = currentUser.FirstName;
                model.LastName = currentUser.LastName;
                return View(model);
            }

            var profile = await _dbContext.Profiles
                .FirstOrDefaultAsync(p => p.UserId == currentUser.Id && p.Id == model.ProfileId);

            if (profile == null)
            {
                return NotFound();
            }

            // Verify this profile belongs to current user
            if (profile.UserId != currentUser.Id)
            {
                return Forbid();
            }

            profile.Birthday = model.Birthday;
            profile.Gender = model.Gender;
            profile.PhoneNumber = model.PhoneNumber;
            profile.Address = model.Address;
            profile.UpdatedAt = DateTime.UtcNow;

            _dbContext.Profiles.Update(profile);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
