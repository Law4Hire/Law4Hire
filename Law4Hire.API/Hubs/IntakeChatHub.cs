using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace Law4Hire.API.Hubs;

/// <summary>
/// SignalR Hub for real-time intake chat functionality
/// </summary>
[Authorize]
[EnableRateLimiting("fixed")]
public class IntakeChatHub : Hub
{
    private readonly ILogger<IntakeChatHub> _logger;

    public IntakeChatHub(ILogger<IntakeChatHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Join an intake session group
    /// </summary>
    /// <param name="sessionId">The intake session ID</param>
    public async Task JoinIntakeSession(string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            throw new HubException("Session ID cannot be empty");
        }

        var userId = Context.UserIdentifier;
        var groupName = $"IntakeSession_{sessionId}";
        
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} joined intake session {SessionId}", userId, sessionId);
        
        await Clients.Group(groupName)
            .SendAsync("UserJoined", userId, DateTime.UtcNow);
    }

    /// <summary>
    /// Leave an intake session group
    /// </summary>
    /// <param name="sessionId">The intake session ID</param>
    public async Task LeaveIntakeSession(string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            throw new HubException("Session ID cannot be empty");
        }

        var userId = Context.UserIdentifier;
        var groupName = $"IntakeSession_{sessionId}";
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} left intake session {SessionId}", userId, sessionId);
        
        await Clients.Group(groupName)
            .SendAsync("UserLeft", userId, DateTime.UtcNow);
    }

    /// <summary>
    /// Send a message to the intake session
    /// </summary>
    /// <param name="sessionId">The intake session ID</param>
    /// <param name="message">The message content</param>
    public async Task SendMessage(string sessionId, string message)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            throw new HubException("Session ID cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new HubException("Message cannot be empty");
        }

        if (message.Length > 1000)
        {
            throw new HubException("Message is too long (max 1000 characters)");
        }

        var userId = Context.UserIdentifier;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";
        var groupName = $"IntakeSession_{sessionId}";
        
        _logger.LogInformation("User {UserId} sent message to session {SessionId}", userId, sessionId);
        
        await Clients.Group(groupName)
            .SendAsync("ReceiveMessage", userId, userName, message, DateTime.UtcNow);
    }

    /// <summary>
    /// Send a bot response with a question
    /// </summary>
    /// <param name="sessionId">The intake session ID</param>
    /// <param name="question">The question text</param>
    /// <param name="questionType">The type of question</param>
    /// <param name="questionId">The question ID</param>
    public async Task SendBotResponse(string sessionId, string question, string questionType, int? questionId = null)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            throw new HubException("Session ID cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(question))
        {
            throw new HubException("Question cannot be empty");
        }

        var groupName = $"IntakeSession_{sessionId}";
        
        _logger.LogInformation("Bot sent question to session {SessionId}", sessionId);
        
        await Clients.Group(groupName)
            .SendAsync("ReceiveBotQuestion", question, questionType, questionId, DateTime.UtcNow);
    }

    /// <summary>
    /// Submit a response to a question
    /// </summary>
    /// <param name="sessionId">The intake session ID</param>
    /// <param name="questionId">The question ID</param>
    /// <param name="response">The user's response</param>
    public async Task SubmitResponse(string sessionId, int questionId, string response)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            throw new HubException("Session ID cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(response))
        {
            throw new HubException("Response cannot be empty");
        }

        var userId = Context.UserIdentifier;
        var groupName = $"IntakeSession_{sessionId}";
        
        _logger.LogInformation("User {UserId} submitted response to question {QuestionId} in session {SessionId}",
            userId, questionId, sessionId);
        
        // TODO: Save response to database and determine next question
        
        await Clients.Group(groupName)
            .SendAsync("ResponseSubmitted", questionId, response, DateTime.UtcNow);
    }

    /// <summary>
    /// Handle connection events
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("User {UserId} connected to IntakeChatHub", userId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Handle disconnection events
    /// </summary>
    /// <param name="exception">Any exception that caused the disconnection</param>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        
        if (exception != null)
        {
            _logger.LogError(exception, "User {UserId} disconnected from IntakeChatHub with error", userId);
        }
        else
        {
            _logger.LogInformation("User {UserId} disconnected from IntakeChatHub", userId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}
