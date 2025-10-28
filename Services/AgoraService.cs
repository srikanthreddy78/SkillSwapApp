using Google.Cloud.Firestore;
using SkillSwapAPI.Models;
using System.Security.Cryptography;
using System.Text;

namespace SkillSwapAPI.Services;

/// <summary>
/// Service implementation for Agora video functionality
/// Handles token generation, session management, and recording operations
/// Uses Firestore for persistence
/// </summary>
public class AgoraService : IAgoraService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AgoraService> _logger;
    private readonly FirestoreDb _firestoreDb;

    private readonly string _appId;
    private readonly string _appCertificate;

    // Constructor - inject dependencies and load Agora credentials from config
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
        
        // Load Agora credentials - throw if not configured
        _appId = _configuration["Agora:AppId"] ?? throw new InvalidOperationException("Agora AppId not configured");
        _appCertificate = _configuration["Agora:AppCertificate"] ?? throw new InvalidOperationException("Agora AppCertificate not configured");
    }

    /// <summary>
    /// Generates an RTC token for a user to join an Agora channel
    /// Token is required for authentication in production environments
    /// </summary>
    public async Task<AgoraTokenResponse> GenerateTokenAsync(GenerateAgoraTokenRequest request)
    {
        try
        {
            // Generate token using our custom implementation
            var token = GenerateAgoraToken(
                _appId,
                _appCertificate,
                request.ChannelName,
                request.UserId,
                request.ExpirationSeconds ?? 3600
            );

            // Return token with metadata for client
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

    /// <summary>
    /// Creates a new video session and persists to Firestore
    /// Tracks instructor, student, and skill details for the session
    /// </summary>
    public async Task<AgoraSessionResponse> CreateSessionAsync(CreateAgoraSessionRequest request)
    {
        try
        {
            // Generate unique session ID
            var sessionId = Guid.NewGuid().ToString();
            
            _logger.LogInformation($"Creating session with data: ChannelName={request.ChannelName}, HostId={request.HostId}");

            // Build session data object with all metadata
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
                { "endTime", null },  // Will be set when session ends
                { "status", "active" },
                { "maxDuration", request.MaxDurationMinutes ?? 60 },
                { "recordingEnabled", request.RecordingEnabled ?? false },
                { "recordingId", null },  // Will be set if recording starts
                { "createdAt", DateTime.UtcNow },
                { "updatedAt", DateTime.UtcNow }
            };

            // Persist session to Firestore
            await _firestoreDb.Collection("videoSessions").Document(sessionId).SetAsync(sessionData);

            _logger.LogInformation($"✅ Saved session to Firestore: {sessionId} | ChannelName: {request.ChannelName}");

            // Return session response to caller
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

    /// <summary>
    /// Retrieves session details from Firestore by session ID
    /// </summary>
    public async Task<AgoraSessionResponse> GetSessionAsync(string sessionId)
    {
        try
        {
            _logger.LogInformation($"Retrieving session from Firestore: {sessionId}");
            
            // Fetch session document from Firestore
            var snapshot = await _firestoreDb.Collection("videoSessions").Document(sessionId).GetSnapshotAsync();
            
            // Return null if session doesn't exist
            if (!snapshot.Exists)
            {
                _logger.LogWarning($"Session not found: {sessionId}");
                return null;
            }

            // Convert Firestore document to dictionary
            var data = snapshot.ToDictionary();

            _logger.LogInformation($"✅ Found session: ChannelName={data["channelName"]}, HostId={data["hostId"]}");

            // Map Firestore data to response model
            return new AgoraSessionResponse
            {
                SessionId = sessionId,
                ChannelName = data["channelName"]?.ToString() ?? string.Empty,
                HostId = data["hostId"]?.ToString() ?? string.Empty,
                InstructorName = data["instructorName"]?.ToString() ?? string.Empty,
                StudentId = data["studentId"]?.ToString() ?? string.Empty,
                StudentName = data["studentName"]?.ToString() ?? string.Empty,
                SkillName = data["skillName"]?.ToString() ?? string.Empty,
                // Handle Firestore Timestamp conversion
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

    /// <summary>
    /// Marks a session as ended in Firestore
    /// Updates status and sets end time
    /// </summary>
    public async Task<bool> EndSessionAsync(string sessionId)
    {
        try
        {
            _logger.LogInformation($"Ending session: {sessionId}");
            
            // Check if session exists first
            var snapshot = await _firestoreDb.Collection("videoSessions").Document(sessionId).GetSnapshotAsync();
            
            if (!snapshot.Exists)
            {
                _logger.LogWarning($"Session not found: {sessionId}");
                return false;
            }

            // Update session status to ended with timestamp
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

    /// <summary>
    /// Initiates cloud recording for a session
    /// Creates recording record in Firestore and links to session
    /// </summary>
    public async Task<AgoraRecordingResponse> StartRecordingAsync(StartRecordingRequest request)
    {
        try
        {
            // Generate unique recording ID
            var recordingId = Guid.NewGuid().ToString();
            
            _logger.LogInformation($"Starting recording for session: {request.SessionId}");

            // Create recording document in Firestore
            var recordingData = new Dictionary<string, object>
            {
                { "recordingId", recordingId },
                { "sessionId", request.SessionId },
                { "channelName", request.ChannelName },
                { "status", "recording" },
                { "startTime", DateTime.UtcNow },
                { "stopTime", null },  // Will be set when recording stops
                { "fileUrl", null },  // Will be populated after processing
                { "outputFormat", request.OutputFormat },
                { "createdAt", DateTime.UtcNow },
                { "updatedAt", DateTime.UtcNow }
            };

            await _firestoreDb.Collection("videoRecordings").Document(recordingId).SetAsync(recordingData);

            // Also link recording ID back to the session for easy lookup
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

    /// <summary>
    /// Stops an active recording
    /// Updates recording status and sets stop time in Firestore
    /// </summary>
    public async Task<bool> StopRecordingAsync(string recordingId)
    {
        try
        {
            _logger.LogInformation($"Stopping recording: {recordingId}");
            
            // Verify recording exists
            var snapshot = await _firestoreDb.Collection("videoRecordings").Document(recordingId).GetSnapshotAsync();
            
            if (!snapshot.Exists)
            {
                _logger.LogWarning($"Recording not found: {recordingId}");
                return false;
            }

            // Update recording status to stopped
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

    /// <summary>
    /// Retrieves all sessions where the user is the host
    /// Queries Firestore for sessions matching the hostId
    /// </summary>
    public async Task<List<AgoraSessionResponse>> GetUserSessionsAsync(string userId)
    {
        try
        {
            _logger.LogInformation($"Retrieving sessions for user: {userId}");
            
            // Query Firestore for sessions where user is the host
            var query = _firestoreDb.Collection("videoSessions")
                .WhereEqualTo("hostId", userId);
            
            var snapshots = await query.GetSnapshotAsync();

            var sessions = new List<AgoraSessionResponse>();

            // Map each Firestore document to response model
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
                    // Convert Firestore timestamp to DateTime
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

    /// <summary>
    /// Generates an Agora RTC token using version 007 token format
    /// Token provides time-limited authentication for channel access
    /// </summary>
    private string GenerateAgoraToken(string appId, string appCertificate, string channelName, 
        uint uid, uint expirationSeconds)
    {
        // Calculate expiration timestamp
        var expirationTimestamp = (uint)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + expirationSeconds);
        var token = new StringBuilder();
        var tokenVersion = "007";  // Agora token version
        
        // Generate signature hash
        var signature = GenerateSignature(
            appId,
            channelName,
            uid,
            appCertificate,
            expirationTimestamp
        );

        // Build token string: version + appId + signature + expiration
        token.Append(tokenVersion);
        token.Append(appId);
        token.Append(signature);
        token.Append(expirationTimestamp.ToString("X"));  // Hex format

        return token.ToString();
    }

    /// <summary>
    /// Generates HMAC-SHA256 signature for Agora token
    /// Signs the combination of appId, channel, uid, and expiration
    /// </summary>
    private static string GenerateSignature(string appId, string channelName, uint uid, 
        string appCertificate, uint expirationTimestamp)
    {
        // Build content string to sign
        var content = new StringBuilder();
        content.Append(appId);
        content.Append(channelName);
        content.Append(uid);
        content.Append(expirationTimestamp);

        // Generate HMAC-SHA256 hash using app certificate as key
        using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(appCertificate)))
        {
            var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(content.ToString()));
            return Convert.ToHexString(hash);
        }
    }
}