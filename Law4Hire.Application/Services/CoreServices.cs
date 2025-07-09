using Law4Hire.Core.Entities;
using Law4Hire.Core.Interfaces;
using Law4Hire.Core.DTOs;
using Law4Hire.Core.Enums;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Law4Hire.Application.Services;

public interface IIntakeService
{
    Task<IntakeSessionDto> StartIntakeSessionAsync(Guid userId, string language = "en-US");
    Task<IntakeQuestionDto?> GetNextQuestionAsync(Guid sessionId);
    Task<bool> SubmitResponseAsync(Guid sessionId, int questionId, string response);
    Task<bool> CompleteIntakeAsync(Guid sessionId);
    Task<IEnumerable<IntakeQuestionDto>> GetAvailableQuestionsAsync(string language = "en-US");
}

public class IntakeService(
    IIntakeSessionRepository sessionRepository,
    IIntakeQuestionRepository questionRepository,
    ILogger<IntakeService> logger) : IIntakeService
{
    private readonly IIntakeSessionRepository _sessionRepository = sessionRepository;
    private readonly IIntakeQuestionRepository _questionRepository = questionRepository;
    private readonly ILogger<IntakeService> _logger = logger;

    public async Task<IntakeSessionDto> StartIntakeSessionAsync(Guid userId, string language = "en-US")
    {
        _logger.LogInformation("Starting intake session for user {UserId} in language {Language}", userId, language);

        var session = new IntakeSession
        {
            UserId = userId,
            Language = language,
            Status = IntakeStatus.Started,
            SessionData = JsonSerializer.Serialize(new { StartedBy = "System", InitialLanguage = language })
        };

        var createdSession = await _sessionRepository.CreateAsync(session);

        _logger.LogInformation("Created intake session {SessionId} for user {UserId}", createdSession.Id, userId);

        return new IntakeSessionDto(
            createdSession.Id,
            createdSession.UserId,
            createdSession.Status,
            createdSession.StartedAt,
            createdSession.CompletedAt,
            createdSession.Language,
            createdSession.SessionData
        );
    }

    public async Task<IntakeQuestionDto?> GetNextQuestionAsync(Guid sessionId)
    {
        _logger.LogInformation("Getting next question for session {SessionId}", sessionId);

        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
        {
            _logger.LogWarning("Session {SessionId} not found", sessionId);
            return null;
        }

        var progress = new UpdateSessionProgressDto(0, new Dictionary<string, string>());
        if (!string.IsNullOrWhiteSpace(session.SessionData))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<UpdateSessionProgressDto>(session.SessionData);
                if (parsed != null)
                {
                    progress = parsed;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse session progress for {SessionId}", sessionId);
            }
        }

        var responses = session.Responses.ToDictionary(r => r.Question.QuestionKey, r => r.ResponseText);
        foreach (var kv in progress.Answers)
            responses[kv.Key] = kv.Value;

        var questions = (await _questionRepository.GetByCategoryAsync("Visit")).ToList();

        foreach (var question in questions)
        {
            if (responses.ContainsKey(question.QuestionKey))
                continue;

            if (!string.IsNullOrWhiteSpace(question.Conditions) && !EvaluateConditions(question.Conditions, responses))
                continue;

            progress = new UpdateSessionProgressDto(progress.CurrentStep + 1, responses);
            session.SessionData = JsonSerializer.Serialize(progress);
            await _sessionRepository.UpdateAsync(session);

            return new IntakeQuestionDto(
                question.Id,
                question.Category,
                question.QuestionKey,
                question.QuestionText,
                question.Type,
                question.Order,
                question.Conditions,
                question.IsRequired,
                question.ValidationRules
            );
        }

        return null;
    }

    public async Task<bool> SubmitResponseAsync(Guid sessionId, int questionId, string response)
    {
        _logger.LogInformation("Submitting response for session {SessionId}, question {QuestionId}", sessionId, questionId);
        
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
        {
            _logger.LogWarning("Session {SessionId} not found", sessionId);
            return false;
        }

        // TODO: Implement response validation and storage
        // TODO: Update session status to InProgress if not already
        
        session.Status = IntakeStatus.InProgress;
        await _sessionRepository.UpdateAsync(session);
        
        _logger.LogInformation("Response submitted successfully for session {SessionId}", sessionId);
        return true;
    }

    public async Task<bool> CompleteIntakeAsync(Guid sessionId)
    {
        _logger.LogInformation("Completing intake session {SessionId}", sessionId);
        
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
        {
            _logger.LogWarning("Session {SessionId} not found", sessionId);
            return false;
        }

        session.Status = IntakeStatus.Completed;
        session.CompletedAt = DateTime.UtcNow;

        await _sessionRepository.UpdateAsync(session);
        
        _logger.LogInformation("Intake session {SessionId} completed successfully", sessionId);
        return true;
    }

    public async Task<IEnumerable<IntakeQuestionDto>> GetAvailableQuestionsAsync(string language = "en-US")
    {
        _logger.LogInformation("Getting available questions for language {Language}", language);
        
        // TODO: Implement database query for questions in specified language
        await Task.Delay(50); // Simulate database call
        
        // Return sample questions for now
        return new[]
        {
            new IntakeQuestionDto(1, "full_name", "What is your full legal name?", QuestionType.Text, 1, null, true, "required,min:2,max:100"),
            new IntakeQuestionDto(2, "date_of_birth", "What is your date of birth?", QuestionType.Date, 2, null, true, "required,date,before:today"),
            new IntakeQuestionDto(3, "country_of_birth", "In which country were you born?", QuestionType.Text, 3, null, true, "required,min:2,max:50")
        };
    }

    private static bool EvaluateConditions(string conditionsJson, IDictionary<string, string> answers)
    {
        try
        {
            using var doc = JsonDocument.Parse(conditionsJson);
            if (doc.RootElement.TryGetProperty("showIf", out var showIf))
            {
                foreach (var condition in showIf.EnumerateObject())
                {
                    var questionKey = condition.Name;
                    if (!answers.TryGetValue(questionKey, out var value))
                        return false;

                    var allowed = condition.Value.EnumerateArray()
                        .Select(v => v.GetString())
                        .Where(v => v != null)
                        .ToHashSet();

                    if (!allowed.Contains(value))
                        return false;
                }
            }
        }
        catch
        {
            // If parsing fails, default to showing the question
        }

        return true;
    }
}

public interface IEncryptionService
{
    string EncryptTransferData(Guid userId, Guid requestId);
    (Guid userId, Guid requestId) DecryptTransferData(string encryptedData);
    string EncryptString(string plainText);
    string DecryptString(string cipherText);
}

public class EncryptionService(
    IConfiguration configuration,
    ILogger<EncryptionService> logger) : IEncryptionService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<EncryptionService> _logger = logger;

    public string EncryptTransferData(Guid userId, Guid requestId)
    {
        _logger.LogInformation("Encrypting transfer data for user {UserId}, request {RequestId}", userId, requestId);
        
        var data = $"{userId}|{requestId}|{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        return EncryptString(data);
    }

    public (Guid userId, Guid requestId) DecryptTransferData(string encryptedData)
    {
        _logger.LogInformation("Decrypting transfer data");
        
        try
        {
            var decryptedData = DecryptString(encryptedData);
            var parts = decryptedData.Split('|');
            
            if (parts.Length < 2)
            {
                throw new InvalidOperationException("Invalid encrypted data format");
            }
            
            var userId = Guid.Parse(parts[0]);
            var requestId = Guid.Parse(parts[1]);
            
            // Optional: Check timestamp if included (parts[2])
            if (parts.Length > 2 && long.TryParse(parts[2], out long timestamp))
            {
                var dataAge = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestamp;
                if (dataAge > 3600) // 1 hour expiration
                {
                    _logger.LogWarning("Encrypted data has expired (age: {Age} seconds)", dataAge);
                    throw new InvalidOperationException("Encrypted data has expired");
                }
            }
            
            _logger.LogInformation("Successfully decrypted transfer data for user {UserId}", userId);
            return (userId, requestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt transfer data");
            throw;
        }
    }

    public string EncryptString(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Plain text cannot be null or empty", nameof(plainText));

        var key = GetEncryptionKey();
        var iv = GetInitializationVector();

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using var writer = new StreamWriter(cs);
        
        writer.Write(plainText);
        writer.Close();
        
        var encryptedBytes = ms.ToArray();
        return Convert.ToBase64String(encryptedBytes);
    }

    public string DecryptString(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            throw new ArgumentException("Cipher text cannot be null or empty", nameof(cipherText));

        var key = GetEncryptionKey();
        var iv = GetInitializationVector();
        var encryptedBytes = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(encryptedBytes);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cs);
        
        return reader.ReadToEnd();
    }

    private byte[] GetEncryptionKey()
    {
        var keyString = _configuration["EncryptionSettings:Key"] ?? 
                       throw new InvalidOperationException("Encryption key not configured");
        
        // Ensure key is exactly 32 bytes for AES-256
        var keyBytes = Encoding.UTF8.GetBytes(keyString);
        if (keyBytes.Length < 32)
        {
            Array.Resize(ref keyBytes, 32);
        }
        else if (keyBytes.Length > 32)
        {
            Array.Resize(ref keyBytes, 32);
        }
        
        return keyBytes;
    }

    private byte[] GetInitializationVector()
    {
        var ivString = _configuration["EncryptionSettings:IV"] ?? 
                      throw new InvalidOperationException("Encryption IV not configured");
        
        // Ensure IV is exactly 16 bytes for AES
        var ivBytes = Encoding.UTF8.GetBytes(ivString);
        if (ivBytes.Length < 16)
        {
            Array.Resize(ref ivBytes, 16);
        }
        else if (ivBytes.Length > 16)
        {
            Array.Resize(ref ivBytes, 16);
        }
        
        return ivBytes;
    }
}

public interface IFormIdentificationService
{
    Task<IEnumerable<string>> IdentifyRequiredFormsAsync(Guid sessionId);
    Task<bool> ValidateFormDataAsync(string formType, string formData);
}

public class FormIdentificationService(ILogger<FormIdentificationService> logger) : IFormIdentificationService
{
    private readonly ILogger<FormIdentificationService> _logger = logger;

    public async Task<IEnumerable<string>> IdentifyRequiredFormsAsync(Guid sessionId)
    {
        _logger.LogInformation("Identifying required forms for session {SessionId}", sessionId);
        
        // TODO: Implement sophisticated form identification logic based on:
        // - User responses from intake session
        // - External form metadata database
        // - Conditional rules and regulations
        
        await Task.Delay(200); // Simulate processing time
        
        // Return sample forms for now
        var sampleForms = new[] { "I-130", "I-485", "I-765" };
        
        _logger.LogInformation("Identified {Count} required forms for session {SessionId}",
            sampleForms.Length, sessionId);
        
        return sampleForms;
    }

    public async Task<bool> ValidateFormDataAsync(string formType, string formData)
    {
        _logger.LogInformation("Validating form data for form type {FormType}", formType);
        
        if (string.IsNullOrWhiteSpace(formType) || string.IsNullOrWhiteSpace(formData))
        {
            _logger.LogWarning("Invalid form type or data provided");
            return false;
        }

        // TODO: Implement form-specific validation rules
        await Task.Delay(100); // Simulate validation time
        
        _logger.LogInformation("Form data validation completed for {FormType}", formType);
        return true;
    }
}
