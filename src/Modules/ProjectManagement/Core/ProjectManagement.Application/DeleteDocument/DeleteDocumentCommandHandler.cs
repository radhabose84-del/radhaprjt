using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Application.Common.Interfaces.IProjectMaster;
using MediatR;

namespace ProjectManagement.Application.DeleteDocument
{
    public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, bool>
    {
        private readonly IFileUploadService _fileUploadService;   
        private readonly IIPAddressService _ipAddressService;
        private readonly IUploadDocumentQueryRepository _uploadDocumentQueryRepository;


        public DeleteDocumentCommandHandler(IFileUploadService fileUploadService, IIPAddressService ipAddressService, IUploadDocumentQueryRepository uploadDocumentQueryRepository)
        {
            _fileUploadService = fileUploadService;
            _ipAddressService = ipAddressService;
           _uploadDocumentQueryRepository = uploadDocumentQueryRepository;
        }

        public async Task<bool> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
        {
            string baseDirectory = await _uploadDocumentQueryRepository.GetDocumentDirectoryAsync();
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                throw new Exception("Base directory not configured.");
            }

            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory);
            string filePath = Path.Combine(uploadPath, request.ProjectDocumentPath ?? string.Empty);
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified file does not exist.", filePath);
            }
            
            var result = await _fileUploadService.DeleteFileAsync(filePath);
            if (!result)
            {
                throw new Exception("File deletion failed.");
            }
            
            if (request.Id > 0 )
            {
                var dbResult = await _uploadDocumentQueryRepository.DeleteFileDetailsDocumentAsync(
                    request.Id,     
                    request.ProjectId,           
                    request.FileName ?? string.Empty
                );

                if (!dbResult)
                {
                    throw new Exception("Document entry not found or deletion failed in DB.");
                }               
            }

            return true;
        }

    }
}