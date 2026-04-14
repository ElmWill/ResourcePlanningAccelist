using MediatR;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjects;

public class CreateProjectAttachmentRequestHandler : IRequestHandler<CreateProjectAttachmentRequest, CreateProjectAttachmentResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public CreateProjectAttachmentRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateProjectAttachmentResponse> Handle(CreateProjectAttachmentRequest request, CancellationToken cancellationToken)
    {
        // Build storage path: uploads/attachments/{projectId}/{guid}_{fileName}
        var uniqueFileName = $"{Guid.NewGuid()}_{request.FileName}";
        var storageKey = Path.Combine("uploads", "attachments", request.ProjectId.ToString(), uniqueFileName);

        // Persist metadata to database
        var attachment = new ProjectAttachment
        {
            ProjectId = request.ProjectId,
            UploadedByUserId = request.UploadedByUserId,
            FileName = request.FileName,
            StorageKey = storageKey,
            ContentType = request.ContentType,
            FileSizeBytes = request.FileSizeBytes
        };

        _dbContext.ProjectAttachments.Add(attachment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateProjectAttachmentResponse
        {
            AttachmentId = attachment.Id,
            FileName = attachment.FileName,
            StorageKey = storageKey,
            FileContent = request.FileContent
        };
    }
}
