using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGateEntry;
using MediatR;

namespace PurchaseManagement.Application.GRN.GateEntry.Commands.DeleteGateEntryDocument
{
    public class DeleteGateEntryDocumentCommandHandler : IRequestHandler<DeleteGateEntryDocumentCommand, bool>
    {
        private readonly IFileUploadService _fileUploadService; 
         private readonly IGateEntryQueryRepository _iGateEntryQueryRepository;

        public DeleteGateEntryDocumentCommandHandler(IFileUploadService fileUploadService, IGateEntryQueryRepository iGateEntryQueryRepository)
        {
            _fileUploadService = fileUploadService;
            _iGateEntryQueryRepository = iGateEntryQueryRepository;
        }

        public async Task<bool> Handle(DeleteGateEntryDocumentCommand request, CancellationToken cancellationToken)
        {
            string baseDirectory = await _iGateEntryQueryRepository.GetDocumentDirectoryAsync();
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                throw new Exception("Base directory not configured.");
            }

            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory);
            string filePath = Path.Combine(uploadPath, request.GateEntrydocumentPath ?? string.Empty);

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