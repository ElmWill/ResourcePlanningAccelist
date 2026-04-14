using System.Text.Json.Serialization;

namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;

public class CreateProjectAttachmentResponse
{
    public Guid AttachmentId { get; set; }

    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Relative storage path used by the controller to write the file to disk.
    /// Excluded from API JSON response.
    /// </summary>
    [JsonIgnore]
    public string StorageKey { get; set; } = string.Empty;

    /// <summary>
    /// Raw file bytes carried from handler back to the controller for disk persistence.
    /// Excluded from API JSON response.
    /// </summary>
    [JsonIgnore]
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
}
