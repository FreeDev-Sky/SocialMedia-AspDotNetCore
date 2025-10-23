using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SocialMedia_AspDotNetCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // This endpoint requires authentication
    public class ProtectedController : ControllerBase
    {
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var name = User.FindFirst(ClaimTypes.Name)?.Value;

            return Ok(new
            {
                UserId = userId,
                Email = email,
                Name = name,
                Message = "This is a protected endpoint. You are authenticated!"
            });
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")] // This endpoint requires Admin role
        public IActionResult GetAdminData()
        {
            return Ok(new
            {
                Message = "This is an admin-only endpoint",
                Data = "Sensitive admin data here"
            });
        }
    }
}
