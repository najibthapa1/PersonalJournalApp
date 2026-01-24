using PersonalJournal.Models.DTO;
using PersonalJournal.Models;

namespace PersonalJournal.Services.Interfaces;

/// <summary>
/// Handles user authentication (login, register, logout)
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user
    /// </summary>
    Task<(bool success, string message, Users? user)> RegisterAsync(RegisterDto registerDto);

    /// <summary>
    /// Authenticates a user and logs them in
    /// </summary>
    Task<(bool success, string message, Users? user)> LoginAsync(LoginDto loginDto);

    /// <summary>
    /// Checks if a username is available
    /// </summary>
    Task<bool> IsUsernameAvailableAsync(string username);

    /// <summary>
    /// Logs out the current user
    /// </summary>
    Task LogoutAsync();
    
    /// <summary>
    /// Updates the PIN for the current user
    /// </summary>
    Task<(bool success, string message)> UpdatePinAsync(string newPin);
}