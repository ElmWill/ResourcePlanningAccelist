using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;

namespace ResourcePlanningAccelist.Commons.Validators.ManageProjects;

public class CreateProjectAttachmentRequestValidator : AbstractValidator<CreateProjectAttachmentRequest>
{
    private static readonly string[] AllowedMimeTypes =
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public CreateProjectAttachmentRequestValidator()
    {
        RuleFor(request => request.ProjectId)
            .NotEmpty();

        RuleFor(request => request.FileName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(request => request.FileContent)
            .NotEmpty()
            .WithMessage("File content must not be empty.");

        RuleFor(request => request.FileSizeBytes)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage($"File size must not exceed {MaxFileSizeBytes / (1024 * 1024)} MB.");

        RuleFor(request => request.ContentType)
            .Must(BeAnAllowedType!)
            .WithMessage("File type is not supported. Allowed types: PDF, DOC, DOCX, XLS, XLSX, and images.");
    }

    private static bool BeAnAllowedType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return false;

        // Allow all image types
        if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return true;

        return AllowedMimeTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase);
    }
}
