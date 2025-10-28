namespace SkillSwapAPI.Models;

/// <summary>
/// Request model for generating an Agora RTC token
/// Used when a user needs to join a video channel
/// </summary>
public class GenerateAgoraTokenRequest
{
    // Name of the Agora channel to join
    public string ChannelName { get; set; } = string.Empty;
    
    // Unique user ID (must be non-zero)
    public uint UserId { get; set; }
    
    // Token expiration time in seconds (default 1 hour)
    public uint? ExpirationSeconds { get; set; } = 3600;
}

/// <summary>
/// Response containing the generated Agora token and related metadata
/// </summary>
public class AgoraTokenResponse
{
    // Generated RTC token for client authentication
    public string Token { get; set; } = string.Empty;
    
    // Channel name the token is valid for
    public string ChannelName { get; set; } = string.Empty;
    
    // User ID associated with this token
    public uint UserId { get; set; }
    
    // Agora application ID for SDK initialization
    public string AppId { get; set; } = string.Empty;
    
    // Token expiration time in seconds
    public uint ExpiresIn { get; set; }
}

/// <summary>
/// Request model for creating a new video session
/// Contains all metadata about the skill exchange session
/// </summary>
public class CreateAgoraSessionRequest
{
    // Unique channel name for this session
    public string ChannelName { get; set; } = string.Empty;
    
    // Host/instructor user ID
    public string HostId { get; set; } = string.Empty;
    
    // Display name of the instructor
    public string InstructorName { get; set; } = string.Empty;
    
    // Student user ID
    public string StudentId { get; set; } = string.Empty;
    
    // Display name of the student
    public string StudentName { get; set; } = string.Empty;
    
    // Name of the skill being taught/exchanged
    public string SkillName { get; set; } = string.Empty;
    
    // Maximum session duration in minutes (default 1 hour)
    public int? MaxDurationMinutes { get; set; } = 60;
    
    // Whether to enable cloud recording for this session
    public bool? RecordingEnabled { get; set; } = false;
}

/// <summary>
/// Response containing session details after creation
/// Used to track active video sessions
/// </summary>
public class AgoraSessionResponse
{
    // Unique identifier for this session
    public string SessionId { get; set; } = string.Empty;
    
    // Agora channel name
    public string ChannelName { get; set; } = string.Empty;
    
    // Host user ID
    public string HostId { get; set; } = string.Empty;
    
    // Instructor display name
    public string InstructorName { get; set; } = string.Empty;
    
    // Student user ID
    public string StudentId { get; set; } = string.Empty;
    
    // Student display name
    public string StudentName { get; set; } = string.Empty;
    
    // Skill being taught
    public string SkillName { get; set; } = string.Empty;
    
    // Session start timestamp
    public DateTime StartTime { get; set; }
    
    // Current session status (active, ended, etc.)
    public string Status { get; set; } = "active";
    
    // Maximum allowed duration in minutes
    public int MaxDuration { get; set; }
    
    // Whether recording is enabled
    public bool RecordingEnabled { get; set; }
}

/// <summary>
/// Request model for starting cloud recording
/// </summary>
public class StartRecordingRequest
{
    // Session ID to record
    public string SessionId { get; set; } = string.Empty;
    
    // Channel name to record from
    public string ChannelName { get; set; } = string.Empty;
    
    // Output file format (default mp4)
    public string OutputFormat { get; set; } = "mp4";
}

/// <summary>
/// Response containing recording details and status
/// </summary>
public class AgoraRecordingResponse
{
    // Unique recording identifier from Agora
    public string RecordingId { get; set; } = string.Empty;
    
    // Associated session ID
    public string SessionId { get; set; } = string.Empty;
    
    // Recording status (recording, stopped, processing, etc.)
    public string Status { get; set; } = string.Empty;
    
    // When recording started
    public DateTime StartTime { get; set; }
    
    // URL to the recorded file (available after processing)
    public string? FileUrl { get; set; }
}

/// <summary>
/// Request model for ending an active session
/// </summary>
public class EndSessionRequest
{
    // Session ID to end
    public string SessionId { get; set; } = string.Empty;
    
    // Optional reason for ending (timeout, user action, error, etc.)
    public string? Reason { get; set; }
}