#nullable disable
using System.ComponentModel.DataAnnotations;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.Application.Item.ItemDetail.Commands.UploadItemImage
{
    public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, ImageDto>
    {
        private readonly ILogger<UploadFileCommandHandler> _logger;        
        private readonly IItemQueryRepository _itemQueryRepository;

        public UploadFileCommandHandler(
            ILogger<UploadFileCommandHandler> logger, IItemQueryRepository itemQueryRepository)
        {
            _logger = logger;            
            _itemQueryRepository = itemQueryRepository;
        }

        public async Task<ImageDto> Handle(UploadFileCommand request, CancellationToken cancellationToken)
        {
            if (request.File == null || request.File.Length == 0)
            {
                throw new ValidationException("No file uploaded");                
            }

             // 🔹 Fetch Base Directory from Database
            string baseDirectory = await _itemQueryRepository.GetBaseDirectoryAsync();
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                _logger.LogError("Base directory path not found in database.");
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

                var response = new ImageDto
                {
                    Image = formattedPath,  // ✅ Correctly formatted file path
                    ImageBase64 = base64Image  // ✅ Convert to Base64
                };
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"File upload failed: {ex.Message}");
                
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
