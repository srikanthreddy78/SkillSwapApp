using SkillSwapAPI.Models;
using System.Security.Cryptography;
using System.Text;
using SkillSwapAPI.Services;

namespace SkillSwapAPI.Services;

public class AgoraService : IAgoraService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AgoraService> _logger;

    private readonly string _appId;
    private readonly string _appCertificate;

    public AgoraService(IConfiguration configuration, HttpClient httpClient, ILogger<AgoraService> logger)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;
        
        _appId = _configuration["Agora:AppId"] ?? throw new InvalidOperationException("Agora AppId not configured");
        _appCertificate = _configuration["Agora:AppCertificate"] ?? throw new InvalidOperationException("Agora AppCertificate not configured");
    }

    public async Task<AgoraTokenResponse> GenerateTokenAsync(GenerateAgoraTokenRequest request)
    {
        try
        {
            var token = GenerateAgoraToken(
                _appId,
                _appCertificate,
                request.ChannelName,
                request.UserId,
                request.ExpirationSeconds ?? 3600
            );

            return new AgoraTokenResponse
            {
                Token = token,
                ChannelName = request.ChannelName,
                UserId = request.UserId,
                AppId = _appId,
                ExpiresIn = request.ExpirationSeconds ?? 3600
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Agora token");
            throw;
        }
    }

    public async Task<AgoraSessionResponse> CreateSessionAsync(CreateAgoraSessionRequest request)
    {
        try
        {
            var sessionId = Guid.NewGuid().ToString();
            
            var session = new AgoraSessionResponse
            {
                SessionId = sessionId,
                ChannelName = request.ChannelName,
                HostId = request.HostId,
                InstructorName = request.InstructorName,
                StudentId = request.StudentId,
                StudentName = request.StudentName,
                SkillName = request.SkillName,
                StartTime = DateTime.UtcNow,
                Status = "active",
                MaxDuration = request.MaxDurationMinutes ?? 60,
                RecordingEnabled = request.RecordingEnabled ?? false
            };

            _logger.LogInformation($"Created Agora session: {sessionId} for channel: {request.ChannelName}");
            
            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Agora session");
            throw;
        }
    }

    public async Task<AgoraSessionResponse> GetSessionAsync(string sessionId)
    {
        try
        {
            _logger.LogInformation($"Retrieving session: {sessionId}");
            
            return new AgoraSessionResponse
            {
                SessionId = sessionId,
                Status = "active"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session");
            throw;
        }
    }

    public async Task<bool> EndSessionAsync(string sessionId)
    {
        try
        {
            _logger.LogInformation($"Ending session: {sessionId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending session");
            throw;
        }
    }

    public async Task<AgoraRecordingResponse> StartRecordingAsync(StartRecordingRequest request)
    {
        try
        {
            var recordingId = Guid.NewGuid().ToString();
            
            var recordingResponse = new AgoraRecordingResponse
            {
                RecordingId = recordingId,
                SessionId = request.SessionId,
                Status = "recording",
                StartTime = DateTime.UtcNow
            };

            _logger.LogInformation($"Started recording: {recordingId} for session: {request.SessionId}");
            
            return recordingResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting recording");
            throw;
        }
    }

    public async Task<bool> StopRecordingAsync(string recordingId)
    {
        try
        {
            _logger.LogInformation($"Stopped recording: {recordingId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping recording");
            throw;
        }
    }

    public async Task<List<AgoraSessionResponse>> GetUserSessionsAsync(string userId)
    {
        try
        {
            _logger.LogInformation($"Retrieving sessions for user: {userId}");
            return new List<AgoraSessionResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user sessions");
            throw;
        }
    }

    private string GenerateAgoraToken(string appId, string appCertificate, string channelName, 
        uint uid, uint expirationSeconds)
    {
        var expirationTimestamp = (uint)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + expirationSeconds);
        var token = new StringBuilder();
        var tokenVersion = "007";
        
        var signature = GenerateSignature(
            appId,
            channelName,
            uid,
            appCertificate,
            expirationTimestamp
        );

        token.Append(tokenVersion);
        token.Append(appId);
        token.Append(signature);
        token.Append(expirationTimestamp.ToString("X"));

        return token.ToString();
    }

    private static string GenerateSignature(string appId, string channelName, uint uid, 
        string appCertificate, uint expirationTimestamp)
    {
        var content = new StringBuilder();
        content.Append(appId);
        content.Append(channelName);
        content.Append(uid);
        content.Append(expirationTimestamp);

        using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(appCertificate)))
        {
            var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(content.ToString()));
            return Convert.ToHexString(hash);
        }
    }
}