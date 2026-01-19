using PersonalJournal.Models;
namespace PersonalJournal.Services.Interfaces;
///<summary>
/// Manages the current logged-in user state across the application
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets the currently logged-in user
    /// </summary>
    Users? CurrentUser { get; }

    /// <summary>
    /// Sets the current user after successful login
    /// </summary>
    void SetCurrentUser(Users user);

    /// <summary>
    /// Clears the current user on logout
    /// </summary>
    void ClearCurrentUser();

    /// <summary>
    /// Checks if a user is currently logged in
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Event fired when user state changes (login/logout)
    /// </summary>
    event Action? OnUserStateChanged;
}