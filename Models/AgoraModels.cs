namespace SkillSwapAPI.Models;

public class GenerateAgoraTokenRequest
{
    public string ChannelName { get; set; } = string.Empty;
    public uint UserId { get; set; }
    public uint? ExpirationSeconds { get; set; } = 3600;
}

public class AgoraTokenResponse
{
    public string Token { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public uint UserId { get; set; }
    public string AppId { get; set; } = string.Empty;
    public uint ExpiresIn { get; set; }
}

public class CreateAgoraSessionRequest
{
    public string ChannelName { get; set; } = string.Empty;
    public string HostId { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public int? MaxDurationMinutes { get; set; } = 60;
    public bool? RecordingEnabled { get; set; } = false;
}

public class AgoraSessionResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public string HostId { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public string Status { get; set; } = "active";
    public int MaxDuration { get; set; }
    public bool RecordingEnabled { get; set; }
}

public class StartRecordingRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public string OutputFormat { get; set; } = "mp4";
}

public class AgoraRecordingResponse
{
    public string RecordingId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public string? FileUrl { get; set; }
}

public class EndSessionRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string? Reason { get; set; }
}