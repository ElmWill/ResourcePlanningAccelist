using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;

public class CreateProjectAttachmentRequest : IRequest<CreateProjectAttachmentResponse>
{
    public Guid ProjectId { get; set; }

    public Guid? UploadedByUserId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string? ContentType { get; set; }

    public long FileSizeBytes { get; set; }

    public byte[] FileContent { get; set; } = Array.Empty<byte>();
}
