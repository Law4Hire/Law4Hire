using Law4Hire.Core.DTOs;
using Microsoft.JSInterop;
using System.Text.Json;

namespace Law4Hire.Web.State;

public class AuthState
{
    private readonly IJSRuntime _jsRuntime;
    private const string USER_KEY = "law4hire_user";
    private const string TOKEN_KEY = "law4hire_token";

    public string SelectedCategory { get; set; }  = string.Empty;
    public bool InterviewStarted { get; set; }
    public bool InterviewCompleted { get; set; }
    public bool InterviewReset { get; set; }

    public UserDto? CurrentUser { get; private set; }
    public string? AuthToken { get; private set; }

    public bool IsLoggedIn => CurrentUser != null && !string.IsNullOrEmpty(AuthToken);

    public event Action? OnChange;

    public AuthState(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Load user and token from localStorage
            var userJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", USER_KEY);
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TOKEN_KEY);

            if (!string.IsNullOrEmpty(userJson) && !string.IsNullOrEmpty(token))
            {
                CurrentUser = JsonSerializer.Deserialize<UserDto>(userJson);
                AuthToken = token;
                NotifyStateChanged();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing auth state: {ex.Message}");
        }
    }

    public async Task SetUserAsync(UserDto user, string token)
    {
        CurrentUser = user;
        AuthToken = token;
        
        try
        {
            // Store user and token in localStorage
            var userJson = JsonSerializer.Serialize(user);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", USER_KEY, userJson);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TOKEN_KEY, token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error storing auth state: {ex.Message}");
        }
        
        NotifyStateChanged();
    }

    public async Task LogoutAsync()
    {
        CurrentUser = null;
        AuthToken = null;
        
        try
        {
            // Remove user and token from localStorage
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", USER_KEY);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TOKEN_KEY);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing auth state: {ex.Message}");
        }
        
        NotifyStateChanged();
    }

    // Legacy methods for backwards compatibility
    public void SetUser(UserDto user)
    {
        _ = SetUserAsync(user, AuthToken ?? "");
    }

    public void Logout()
    {
        _ = LogoutAsync();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
