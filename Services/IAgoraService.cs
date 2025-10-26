using SkillSwapAPI.Models;

namespace SkillSwapAPI.Services;

public interface IAgoraService
{
    /// <summary>
    /// Generate an Agora access token for a user to join a channel
    /// </summary>
    Task<AgoraTokenResponse> GenerateTokenAsync(GenerateAgoraTokenRequest request);

    /// <summary>
    /// Create a video session (room)
    /// </summary>
    Task<AgoraSessionResponse> CreateSessionAsync(CreateAgoraSessionRequest request);

    /// <summary>
    /// Get session details
    /// </summary>
    Task<AgoraSessionResponse> GetSessionAsync(string sessionId);

    /// <summary>
    /// End a video session
    /// </summary>
    Task<bool> EndSessionAsync(string sessionId);

    /// <summary>
    /// Record a video session
    /// </summary>
    Task<AgoraRecordingResponse> StartRecordingAsync(StartRecordingRequest request);

    /// <summary>
    /// Stop recording a session
    /// </summary>
    Task<bool> StopRecordingAsync(string recordingId);

    /// <summary>
    /// Get user's active sessions
    /// </summary>
    Task<List<AgoraSessionResponse>> GetUserSessionsAsync(string userId);
}