using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialMedia_AspDotNetCore.Models;

namespace SocialMedia_AspDotNetCore.Hubs
{
    public class ChatHub : Hub
    {
        private readonly UserManager<User> _userManager;
        private readonly IServiceProvider _serviceProvider;

        public ChatHub(UserManager<User> userManager, IServiceProvider serviceProvider)
        {
            _userManager = userManager;
            _serviceProvider = serviceProvider;
        }

        // Send global message (to everyone)
        public async Task SendMessage(string message)
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("ReceiveError", "You must be logged in to send messages.");
                return;
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                await Clients.Caller.SendAsync("ReceiveError", "User not found.");
                return;
            }

            // Create a scope for DbContext since Hub is singleton
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Save message to database (global message - no ReceiverId)
                var dbMessage = new Message
                {
                    UserId = userId,
                    Content = message,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.Messages.Add(dbMessage);
                await dbContext.SaveChangesAsync();

                // Get user display name
                var displayName = !string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.LastName)
                    ? $"{user.FirstName} {user.LastName}"
                    : user.Email ?? "Unknown User";

                // Broadcast message to all connected clients (only global messages)
                // Make sure ReceiverId is explicitly null for global messages
                await Clients.All.SendAsync("ReceiveMessage", new
                {
                    id = dbMessage.Id,
                    userId = userId,
                    userName = displayName,
                    content = message,
                    createdAt = dbMessage.CreatedAt,
                    isPrivate = false,
                    receiverId = (string?)null  // Explicitly null for global messages
                });
            }
        }

        // Send private message to a specific user
        public async Task SendPrivateMessage(string receiverId, string message)
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("ReceiveError", "You must be logged in to send messages.");
                return;
            }

            if (userId == receiverId)
            {
                await Clients.Caller.SendAsync("ReceiveError", "You cannot send a message to yourself.");
                return;
            }

            var user = await _userManager.FindByIdAsync(userId);
            var receiver = await _userManager.FindByIdAsync(receiverId);

            if (user == null || receiver == null)
            {
                await Clients.Caller.SendAsync("ReceiveError", "User not found.");
                return;
            }

            // Create a scope for DbContext since Hub is singleton
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Save private message to database
                var dbMessage = new Message
                {
                    UserId = userId,
                    ReceiverId = receiverId,
                    Content = message,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.Messages.Add(dbMessage);
                await dbContext.SaveChangesAsync();

                // Get user display names
                var senderName = !string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.LastName)
                    ? $"{user.FirstName} {user.LastName}"
                    : user.Email ?? "Unknown User";

                var receiverName = !string.IsNullOrEmpty(receiver.FirstName) && !string.IsNullOrEmpty(receiver.LastName)
                    ? $"{receiver.FirstName} {receiver.LastName}"
                    : receiver.Email ?? "Unknown User";

                var messageData = new
                {
                    id = dbMessage.Id,
                    userId = userId,
                    userName = senderName,
                    receiverId = receiverId,
                    receiverName = receiverName,
                    content = message,
                    createdAt = dbMessage.CreatedAt,
                    isPrivate = true
                };

                // Send to sender
                await Clients.Caller.SendAsync("ReceivePrivateMessage", messageData);

                // Send to receiver (if connected)
                await Clients.User(receiverId).SendAsync("ReceivePrivateMessage", messageData);
            }
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = Context.UserIdentifier;
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        var displayName = !string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.LastName)
                            ? $"{user.FirstName} {user.LastName}"
                            : user.Email ?? "Unknown User";

                        await Clients.Others.SendAsync("UserJoined", displayName);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the connection
                Console.WriteLine($"Error in OnConnectedAsync: {ex.Message}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var displayName = !string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.LastName)
                        ? $"{user.FirstName} {user.LastName}"
                        : user.Email ?? "Unknown User";

                    await Clients.Others.SendAsync("UserLeft", displayName);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}

