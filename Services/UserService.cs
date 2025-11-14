using Grpc.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialMedia_AspDotNetCore.Models;
using SocialMedia_AspDotNetCore.Protos;

namespace SocialMedia_AspDotNetCore.Services
{
    public class UserService : Protos.UserService.UserServiceBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserService> _logger;

        public UserService(
            ApplicationDbContext context,
            UserManager<User> userManager,
            ILogger<UserService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public override async Task<UserResponse> GetUser(GetUserRequest request, ServerCallContext context)
        {
            _logger.LogInformation("GetUser called with ID: {UserId}", request.UserId);

            var user = await _userManager.FindByIdAsync(request.UserId);
            
            if (user == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"User with ID {request.UserId} not found"));
            }

            return new UserResponse
            {
                UserId = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow,
                CreatedAt = user.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
        }

        public override async Task<GetAllUsersResponse> GetAllUsers(GetAllUsersRequest request, ServerCallContext context)
        {
            _logger.LogInformation("GetAllUsers called with page: {PageNumber}, size: {PageSize}", 
                request.PageNumber, request.PageSize);

            var pageNumber = request.PageNumber > 0 ? request.PageNumber : 1;
            var pageSize = request.PageSize > 0 ? request.PageSize : 10;

            var totalCount = await _userManager.Users.CountAsync();
            var users = await _userManager.Users
                .OrderBy(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new GetAllUsersResponse
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            response.Users.AddRange(users.Select(user => new UserResponse
            {
                UserId = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow,
                CreatedAt = user.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
            }));

            return response;
        }

        public override async Task<GetUserCountResponse> GetUserCount(GetUserCountRequest request, ServerCallContext context)
        {
            _logger.LogInformation("GetUserCount called");

            var count = await _userManager.Users.CountAsync();

            return new GetUserCountResponse
            {
                Count = count
            };
        }
    }
}

