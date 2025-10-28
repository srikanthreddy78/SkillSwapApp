using System.ComponentModel.DataAnnotations;

namespace SkillSwapAPI.Models;

/// <summary>
/// Entity model representing a video recording stored in the database
/// Tracks Agora cloud recording sessions and their metadata
/// </summary>
public class VideoRecording
{
    // Primary key - unique recording identifier from Agora
    [Key]
    public string RecordingId { get; set; } = string.Empty;

    // Foreign key linking to the video session
    [Required]
    [StringLength(255)]
    public string SessionId { get; set; } = string.Empty;

    // Agora channel name that was recorded
    [StringLength(255)]
    public string ChannelName { get; set; } = string.Empty;

    // Current recording status (recording, processing, completed, failed, etc.)
    [StringLength(50)]
    public string Status { get; set; } = "recording";

    // When the recording started
    public DateTime StartTime { get; set; }

    // When the recording stopped (null if still recording)
    public DateTime? StopTime { get; set; }

    // URL to the stored recording file (populated after processing completes)
    [StringLength(255)]
    public string? FileUrl { get; set; }

    // File format of the recording (mp4, webm, etc.)
    [StringLength(50)]
    public string OutputFormat { get; set; } = "mp4";

    // Database record creation timestamp
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Last time this record was updated
    public DateTime? UpdatedAt { get; set; }
}