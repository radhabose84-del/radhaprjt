#nullable disable
using System.ComponentModel.DataAnnotations;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.UploadGRNDocument
{
    public class UploadGrnQcDocumentCommandHandler : IRequestHandler<UploadGrnQcDocumentCommand, GRNQcImageDto>
    {
        private readonly IGRNEntryQueryRepository _iGrnEntryQueryRepository;
        public UploadGrnQcDocumentCommandHandler(IGRNEntryQueryRepository iGrnEntryQueryRepository)
        {
            _iGrnEntryQueryRepository = iGrnEntryQueryRepository;
        }

        public async Task<GRNQcImageDto> Handle(UploadGrnQcDocumentCommand request, CancellationToken cancellationToken)
        {
            if (request.File == null || request.File.Length == 0)
            {
                throw new ValidationException("No file uploaded");

            }
            // 🔹 Fetch Base Directory from Database
            string baseDirectory = await _iGrnEntryQueryRepository.GetDocumentDirectoryAsync();
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {

                throw new ValidationException("Base directory not configured.");

            }
            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory);
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

                var response = new GRNQcImageDto
                {
                    ImagePath = formattedPath,  // ✅ Correctly formatted file path
                    GrnQcEntryDocumentBase64 = base64Image  // ✅ Convert to Base64
                };
                return response;
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