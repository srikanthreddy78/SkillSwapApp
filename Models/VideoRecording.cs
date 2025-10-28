using System.ComponentModel.DataAnnotations;

namespace SkillSwapAPI.Models;

public class VideoRecording
{
    [Key]
    public string RecordingId { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string SessionId { get; set; } = string.Empty;

    [StringLength(255)]
    public string ChannelName { get; set; } = string.Empty;

    [StringLength(50)]
    public string Status { get; set; } = "recording";

    public DateTime StartTime { get; set; }

    public DateTime? StopTime { get; set; }

    [StringLength(255)]
    public string? FileUrl { get; set; }

    [StringLength(50)]
    public string OutputFormat { get; set; } = "mp4";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}