using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwapAPI.Models;
using SkillSwapAPI.Services;

namespace SkillSwapAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class VideoController : ControllerBase
{
    private readonly IAgoraService _agoraService;
    private readonly ILogger<VideoController> _logger;

    public VideoController(IAgoraService agoraService, ILogger<VideoController> logger)
    {
        _agoraService = agoraService;
        _logger = logger;
    }

    [HttpPost("generate-token")]
    public async Task<IActionResult> GenerateToken([FromBody] GenerateAgoraTokenRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.ChannelName))
                return BadRequest(new { message = "Channel name is required" });

            if (request.UserId == 0)
                return BadRequest(new { message = "User ID is required" });

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

    [HttpPost("create-session")]
    public async Task<IActionResult> CreateSession([FromBody] CreateAgoraSessionRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.ChannelName))
                return BadRequest(new { message = "Channel name is required" });

            if (string.IsNullOrEmpty(request.HostId))
                return BadRequest(new { message = "Host ID is required" });

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

    [HttpGet("session/{sessionId}")]
    public async Task<IActionResult> GetSession(string sessionId)
    {
        try
        {
            if (string.IsNullOrEmpty(sessionId))
                return BadRequest(new { message = "Session ID is required" });

            var result = await _agoraService.GetSessionAsync(sessionId);
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

    [HttpPost("end-session")]
    public async Task<IActionResult> EndSession([FromBody] EndSessionRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.SessionId))
                return BadRequest(new { message = "Session ID is required" });

            var result = await _agoraService.EndSessionAsync(request.SessionId);
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

    [HttpPost("start-recording")]
    public async Task<IActionResult> StartRecording([FromBody] StartRecordingRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.SessionId))
                return BadRequest(new { message = "Session ID is required" });

            if (string.IsNullOrEmpty(request.ChannelName))
                return BadRequest(new { message = "Channel name is required" });

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

    [HttpPost("stop-recording/{recordingId}")]
    public async Task<IActionResult> StopRecording(string recordingId)
    {
        try
        {
            if (string.IsNullOrEmpty(recordingId))
                return BadRequest(new { message = "Recording ID is required" });

            var result = await _agoraService.StopRecordingAsync(recordingId);
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

    [HttpGet("user/{userId}/sessions")]
    public async Task<IActionResult> GetUserSessions(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { message = "User ID is required" });

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