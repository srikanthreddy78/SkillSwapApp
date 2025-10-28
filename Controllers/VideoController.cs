using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwapAPI.Models;
using SkillSwapAPI.Services;

namespace SkillSwapAPI.Controllers;

/// <summary>
/// Controller for handling Agora video call operations
/// Manages token generation, session lifecycle, and recording functionality
/// </summary>
[ApiController]
[Route("api/[controller]")]
// [Authorize] // TODO: Enable authorization once auth is fully implemented
public class VideoController : ControllerBase
{
    private readonly IAgoraService _agoraService;
    private readonly ILogger<VideoController> _logger;

    // Constructor - DI injection for Agora service and logging
    public VideoController(IAgoraService agoraService, ILogger<VideoController> logger)
    {
        _agoraService = agoraService;
        _logger = logger;
    }

    /// <summary>
    /// Generates an Agora RTC token for a user to join a video channel
    /// POST: api/video/generate-token
    /// </summary>
    [HttpPost("generate-token")]
    public async Task<IActionResult> GenerateToken([FromBody] GenerateAgoraTokenRequest request)
    {
        try
        {
            // Validate channel name
            if (string.IsNullOrEmpty(request.ChannelName))
                return BadRequest(new { message = "Channel name is required" });

            // Validate user ID - must be non-zero
            if (request.UserId == 0)
                return BadRequest(new { message = "User ID is required" });

            // Generate token via Agora service
            var result = await _agoraService.GenerateTokenAsync(request);
            _logger.LogInformation($"Generated token for user {request.UserId}");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating token");
            return StatusCode(500, new { message = "Failed to generate token", error = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new video call session
    /// POST: api/video/create-session
    /// </summary>
    [HttpPost("create-session")]
    public async Task<IActionResult> CreateSession([FromBody] CreateAgoraSessionRequest request)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrEmpty(request.ChannelName))
                return BadRequest(new { message = "Channel name is required" });

            if (string.IsNullOrEmpty(request.HostId))
                return BadRequest(new { message = "Host ID is required" });

            // Create session and return session details
            var result = await _agoraService.CreateSessionAsync(request);
            _logger.LogInformation($"Created session: {result.SessionId}");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session");
            return StatusCode(500, new { message = "Failed to create session", error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves session details by session ID
    /// GET: api/video/session/{sessionId}
    /// </summary>
    [HttpGet("session/{sessionId}")]
    public async Task<IActionResult> GetSession(string sessionId)
    {
        try
        {
            // Validate session ID parameter
            if (string.IsNullOrEmpty(sessionId))
                return BadRequest(new { message = "Session ID is required" });

            var result = await _agoraService.GetSessionAsync(sessionId);
            
            // Return 404 if session doesn't exist
            if (result == null)
                return NotFound(new { message = "Session not found" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session");
            return StatusCode(500, new { message = "Failed to retrieve session", error = ex.Message });
        }
    }

    /// <summary>
    /// Ends an active video session
    /// POST: api/video/end-session
    /// </summary>
    [HttpPost("end-session")]
    public async Task<IActionResult> EndSession([FromBody] EndSessionRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.SessionId))
                return BadRequest(new { message = "Session ID is required" });

            var result = await _agoraService.EndSessionAsync(request.SessionId);
            
            // Check if session was successfully ended
            if (!result)
                return BadRequest(new { message = "Failed to end session" });

            _logger.LogInformation($"Ended session: {request.SessionId}");
            return Ok(new { message = "Session ended successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending session");
            return StatusCode(500, new { message = "Failed to end session", error = ex.Message });
        }
    }

    /// <summary>
    /// Starts cloud recording for a session
    /// POST: api/video/start-recording
    /// </summary>
    [HttpPost("start-recording")]
    public async Task<IActionResult> StartRecording([FromBody] StartRecordingRequest request)
    {
        try
        {
            // Validate required parameters for recording
            if (string.IsNullOrEmpty(request.SessionId))
                return BadRequest(new { message = "Session ID is required" });

            if (string.IsNullOrEmpty(request.ChannelName))
                return BadRequest(new { message = "Channel name is required" });

            // Initiate cloud recording via Agora
            var result = await _agoraService.StartRecordingAsync(request);
            _logger.LogInformation($"Started recording: {result.RecordingId}");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting recording");
            return StatusCode(500, new { message = "Failed to start recording", error = ex.Message });
        }
    }

    /// <summary>
    /// Stops an active recording
    /// POST: api/video/stop-recording/{recordingId}
    /// </summary>
    [HttpPost("stop-recording/{recordingId}")]
    public async Task<IActionResult> StopRecording(string recordingId)
    {
        try
        {
            if (string.IsNullOrEmpty(recordingId))
                return BadRequest(new { message = "Recording ID is required" });

            var result = await _agoraService.StopRecordingAsync(recordingId);
            
            // Verify recording was stopped successfully
            if (!result)
                return BadRequest(new { message = "Failed to stop recording" });

            _logger.LogInformation($"Stopped recording: {recordingId}");
            return Ok(new { message = "Recording stopped successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping recording");
            return StatusCode(500, new { message = "Failed to stop recording", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all sessions for a specific user
    /// GET: api/video/user/{userId}/sessions
    /// </summary>
    [HttpGet("user/{userId}/sessions")]
    public async Task<IActionResult> GetUserSessions(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { message = "User ID is required" });

            // Retrieve session history for the user
            var result = await _agoraService.GetUserSessionsAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sessions");
            return StatusCode(500, new { message = "Failed to retrieve sessions", error = ex.Message });
        }
    }
}