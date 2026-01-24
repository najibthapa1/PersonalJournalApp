using PersonalJournal.Data;
using PersonalJournal.Models;
using PersonalJournal.Models.DTO;
using PersonalJournal.Models.Enums;
using PersonalJournal.Services.Interfaces;
using PersonalJournal.Utilities;
using Microsoft.EntityFrameworkCore;

namespace PersonalJournal.Services.Implementation;

public class AuthService: IAuthService
{
     private readonly AppDbContext _context;
        private readonly IUserService _userService;

        public AuthService(AppDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<(bool success, string message, Users? user)> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Check if username already exists
                var usernameExists = await _context.Users
                    .AnyAsync(u => u.Username.ToLower() == registerDto.Username.ToLower());

                if (usernameExists)
                {
                    return (false, "Username already exists. Please choose a different username.", null);
                }

                // Hash the PIN
                var (hash, salt) = PinHasher.HashPin(registerDto.Pin);

                // Create new user
                var newUser = new Users
                {
                    Username = registerDto.Username,
                    PinHash = hash,
                    Salt = salt,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // Create default user settings
                var userSettings = new UserSettings
                {
                    UserId = newUser.Id,
                    ThemeMode = ThemeMode.Light
                };

                _context.UserSettings.Add(userSettings);
                await _context.SaveChangesAsync();

                return (true, "Registration successful!", newUser);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}", null);
            }
        }

        public async Task<(bool success, string message, Users? user)> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Find user by username
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == loginDto.Username.ToLower());

                if (user == null)
                {
                    return (false, "Invalid username or PIN.", null);
                }

                // Verify PIN
                bool isPinValid = PinHasher.VerifyPin(loginDto.Pin, user.PinHash, user.Salt);

                if (!isPinValid)
                {
                    return (false, "Invalid username or PIN.", null);
                }

                // Update last login time
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Set current user in state service
                _userService.SetCurrentUser(user);

                return (true, "Login successful!", user);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}", null);
            }
        }

        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            return !await _context.Users
                .AnyAsync(u => u.Username.ToLower() == username.ToLower());
        }

        public Task LogoutAsync()
        {
            _userService.ClearCurrentUser();
            return Task.CompletedTask;
        }

        public async Task<(bool success, string message)> UpdatePinAsync(string newPin)
        {
            try
            {
                // Check if user is authenticated
                if (_userService.CurrentUser == null)
                {
                    return (false, "User not authenticated");
                }

                // Validate PIN format
                if (string.IsNullOrWhiteSpace(newPin))
                {
                    return (false, "PIN cannot be empty");
                }

                if (newPin.Length < 4 || newPin.Length > 6)
                {
                    return (false, "PIN must be 4-6 digits");
                }

                if (!newPin.All(char.IsDigit))
                {
                    return (false, "PIN must contain only numbers");
                }

                // Get user from database
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == _userService.CurrentUser.Id);

                if (user == null)
                {
                    return (false, "User not found");
                }

                // Hash the new PIN
                var (hash, salt) = PinHasher.HashPin(newPin);

                // Update  PIN
                user.PinHash = hash;
                user.Salt = salt;

                await _context.SaveChangesAsync();

                return (true, "PIN updated successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating PIN: {ex.Message}");
            }
        }
}