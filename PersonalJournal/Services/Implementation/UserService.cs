using PersonalJournal.Models;
using PersonalJournal.Services.Interfaces;
namespace PersonalJournal.Services.Implementation;

/// <summary>
/// Manages the current logged-in user state
/// </summary>
public class UserService :  IUserService
{
    private Users? _currentUser;

    public Users? CurrentUser => _currentUser;

    public bool IsAuthenticated => _currentUser != null;

    public event Action? OnUserStateChanged;

    public void SetCurrentUser(Users user)
    {
        _currentUser = user;
        OnUserStateChanged?.Invoke();
    }

    public void ClearCurrentUser()
    {
        _currentUser = null;
        OnUserStateChanged?.Invoke();
    }
}