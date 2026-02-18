#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ProjectManagement.Application.Common.Interfaces.IProjectMaster;
using ProjectManagement.Domain.Common;
using MediatR;

namespace ProjectManagement.Application.UploadDocument
{
    public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, DocumentDto>
    {

        private readonly IUploadDocumentQueryRepository _uploadDocumentQueryRepository;

        public UploadDocumentCommandHandler(IUploadDocumentQueryRepository uploadDocumentQueryRepository)
        {
            _uploadDocumentQueryRepository = uploadDocumentQueryRepository;
        }

        public async Task<DocumentDto> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
        {
           if (request.File == null || request.File.Length == 0)
            {
                throw new ValidationException("No file uploaded");
            }
            // 🔹 Fetch Base Directory from Database
            string baseDirectory = MiscEnumEntity.DocumentPath; // await _poDocumentQueryRepository.GetDocumentDirectoryAsync();
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {

                throw new ValidationException("Base directory not configured.");

            }
            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(),"Resources",baseDirectory);
            EnsureDirectoryExists(uploadPath);
            string fileExtension = Path.GetExtension(request.File.FileName);            
            string dummyFileName = $"TEMP_{Guid.NewGuid()}{fileExtension}";
            string filePath = Path.Combine(uploadPath, dummyFileName);
            try
            {
                EnsureDirectoryExists(Path.GetDirectoryName(filePath));
                // Save the file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(fileStream);
                }

                // Convert Image to Base64 (optional)
                string base64Image = Convert.ToBase64String(await File.ReadAllBytesAsync(filePath));

                // ✅ Ensure the correct format before saving in DB
                string formattedPath = dummyFileName;

                var response = new DocumentDto
                {
                    FileName = formattedPath,  // ✅ Correctly formatted file path
                    PODocumentBase64 = base64Image  // ✅ Convert to Base64
                };
                return  response;
            }
            catch (Exception ex)
            {
               
                throw new Exception($"File upload failed: {ex.Message}");
                
            }
        }
        
         private void EnsureDirectoryExists(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}