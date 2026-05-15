using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.DeleteGRNDocument
{
    public class DeleteGrnDetailDocumentCommandHandler : IRequestHandler<DeleteGrnDetailDocumentCommand, bool>
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IGRNEntryQueryRepository _iGrnEntryQueryRepository;

        public DeleteGrnDetailDocumentCommandHandler(IFileUploadService fileUploadService, IGRNEntryQueryRepository iGrnEntryQueryRepository)
        {
            _fileUploadService = fileUploadService;
            _iGrnEntryQueryRepository = iGrnEntryQueryRepository;
        }

        public async Task<bool> Handle(DeleteGrnDetailDocumentCommand request, CancellationToken cancellationToken)
        {
            string baseDirectory = await _iGrnEntryQueryRepository.GetDocumentDirectoryAsync();
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                throw new Exception("Base directory not configured.");
            }

            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory);
            string filePath = Path.Combine(uploadPath, request.GrnDetaildocumentPath ?? string.Empty);

            // ✅ Check if file exists before attempting delete
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified file does not exist.", filePath);
            }

            // ✅ First delete file physically
            var result = await _fileUploadService.DeleteFileAsync(filePath);
            if (!result)
            {
                throw new Exception("File deletion failed.");
            }
            return true;
        }
    }
}
