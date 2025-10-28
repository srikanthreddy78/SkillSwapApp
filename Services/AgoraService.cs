using Google.Cloud.Firestore;
using SkillSwapAPI.Models;
using System.Security.Cryptography;
using System.Text;

namespace SkillSwapAPI.Services;

public class AgoraService : IAgoraService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AgoraService> _logger;
    private readonly FirestoreDb _firestoreDb;

    private readonly string _appId;
    private readonly string _appCertificate;

    public AgoraService(
        IConfiguration configuration,
        HttpClient httpClient,
        ILogger<AgoraService> logger,
        FirestoreDb firestoreDb)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;
        _firestoreDb = firestoreDb;
        
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
            
            _logger.LogInformation($"Creating session with data: ChannelName={request.ChannelName}, HostId={request.HostId}");

            // ✅ Create session object with all data
            var sessionData = new Dictionary<string, object>
            {
                { "sessionId", sessionId },
                { "channelName", request.ChannelName },
                { "hostId", request.HostId },
                { "instructorName", request.InstructorName },
                { "studentId", request.StudentId },
                { "studentName", request.StudentName },
                { "skillName", request.SkillName },
                { "startTime", DateTime.UtcNow },
                { "endTime", null },
                { "status", "active" },
                { "maxDuration", request.MaxDurationMinutes ?? 60 },
                { "recordingEnabled", request.RecordingEnabled ?? false },
                { "recordingId", null },
                { "createdAt", DateTime.UtcNow },
                { "updatedAt", DateTime.UtcNow }
            };

            // ✅ SAVE TO FIRESTORE
            await _firestoreDb.Collection("videoSessions").Document(sessionId).SetAsync(sessionData);

            _logger.LogInformation($"✅ Saved session to Firestore: {sessionId} | ChannelName: {request.ChannelName}");

            // ✅ Return response
            return new AgoraSessionResponse
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
            _logger.LogInformation($"Retrieving session from Firestore: {sessionId}");
            
            // ✅ GET FROM FIRESTORE
            var snapshot = await _firestoreDb.Collection("videoSessions").Document(sessionId).GetSnapshotAsync();
            
            if (!snapshot.Exists)
            {
                _logger.LogWarning($"Session not found: {sessionId}");
                return null;
            }

            var data = snapshot.ToDictionary();

            _logger.LogInformation($"✅ Found session: ChannelName={data["channelName"]}, HostId={data["hostId"]}");

            return new AgoraSessionResponse
            {
                SessionId = sessionId,
                ChannelName = data["channelName"]?.ToString() ?? string.Empty,
                HostId = data["hostId"]?.ToString() ?? string.Empty,
                InstructorName = data["instructorName"]?.ToString() ?? string.Empty,
                StudentId = data["studentId"]?.ToString() ?? string.Empty,
                StudentName = data["studentName"]?.ToString() ?? string.Empty,
                SkillName = data["skillName"]?.ToString() ?? string.Empty,
                StartTime = (data.ContainsKey("startTime") && data["startTime"] is Timestamp ts) 
                    ? ts.ToDateTime() 
                    : DateTime.UtcNow,
                Status = data["status"]?.ToString() ?? "active",
                MaxDuration = data.ContainsKey("maxDuration") ? int.Parse(data["maxDuration"].ToString()) : 60,
                RecordingEnabled = data.ContainsKey("recordingEnabled") && bool.Parse(data["recordingEnabled"].ToString())
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
            
            // ✅ GET FROM FIRESTORE
            var snapshot = await _firestoreDb.Collection("videoSessions").Document(sessionId).GetSnapshotAsync();
            
            if (!snapshot.Exists)
            {
                _logger.LogWarning($"Session not found: {sessionId}");
                return false;
            }

            // ✅ UPDATE IN FIRESTORE
            await _firestoreDb.Collection("videoSessions").Document(sessionId).UpdateAsync(new Dictionary<string, object>
            {
                { "status", "ended" },
                { "endTime", DateTime.UtcNow },
                { "updatedAt", DateTime.UtcNow }
            });

            _logger.LogInformation($"✅ Session ended: {sessionId}");
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
            
            _logger.LogInformation($"Starting recording for session: {request.SessionId}");

            // ✅ CREATE RECORDING IN FIRESTORE
            var recordingData = new Dictionary<string, object>
            {
                { "recordingId", recordingId },
                { "sessionId", request.SessionId },
                { "channelName", request.ChannelName },
                { "status", "recording" },
                { "startTime", DateTime.UtcNow },
                { "stopTime", null },
                { "fileUrl", null },
                { "outputFormat", request.OutputFormat },
                { "createdAt", DateTime.UtcNow },
                { "updatedAt", DateTime.UtcNow }
            };

            await _firestoreDb.Collection("videoRecordings").Document(recordingId).SetAsync(recordingData);

            // ✅ ALSO UPDATE SESSION WITH RECORDING ID
            await _firestoreDb.Collection("videoSessions").Document(request.SessionId).UpdateAsync(new Dictionary<string, object>
            {
                { "recordingId", recordingId },
                { "updatedAt", DateTime.UtcNow }
            });

            _logger.LogInformation($"✅ Recording started: {recordingId}");

            return new AgoraRecordingResponse
            {
                RecordingId = recordingId,
                SessionId = request.SessionId,
                Status = "recording",
                StartTime = DateTime.UtcNow
            };
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
            _logger.LogInformation($"Stopping recording: {recordingId}");
            
            // ✅ GET FROM FIRESTORE
            var snapshot = await _firestoreDb.Collection("videoRecordings").Document(recordingId).GetSnapshotAsync();
            
            if (!snapshot.Exists)
            {
                _logger.LogWarning($"Recording not found: {recordingId}");
                return false;
            }

            // ✅ UPDATE IN FIRESTORE
            await _firestoreDb.Collection("videoRecordings").Document(recordingId).UpdateAsync(new Dictionary<string, object>
            {
                { "status", "stopped" },
                { "stopTime", DateTime.UtcNow },
                { "updatedAt", DateTime.UtcNow }
            });

            _logger.LogInformation($"✅ Recording stopped: {recordingId}");
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
            
            // ✅ GET FROM FIRESTORE - query where hostId matches
            var query = _firestoreDb.Collection("videoSessions")
                .WhereEqualTo("hostId", userId);
            
            var snapshots = await query.GetSnapshotAsync();

            var sessions = new List<AgoraSessionResponse>();

            foreach (var snapshot in snapshots)
            {
                var data = snapshot.ToDictionary();
                sessions.Add(new AgoraSessionResponse
                {
                    SessionId = data["sessionId"]?.ToString() ?? string.Empty,
                    ChannelName = data["channelName"]?.ToString() ?? string.Empty,
                    HostId = data["hostId"]?.ToString() ?? string.Empty,
                    InstructorName = data["instructorName"]?.ToString() ?? string.Empty,
                    StudentId = data["studentId"]?.ToString() ?? string.Empty,
                    StudentName = data["studentName"]?.ToString() ?? string.Empty,
                    SkillName = data["skillName"]?.ToString() ?? string.Empty,
                    StartTime = (data.ContainsKey("startTime") && data["startTime"] is Timestamp ts) 
                        ? ts.ToDateTime() 
                        : DateTime.UtcNow,
                    Status = data["status"]?.ToString() ?? "active",
                    MaxDuration = data.ContainsKey("maxDuration") ? int.Parse(data["maxDuration"].ToString()) : 60,
                    RecordingEnabled = data.ContainsKey("recordingEnabled") && bool.Parse(data["recordingEnabled"].ToString())
                });
            }

            return sessions;
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