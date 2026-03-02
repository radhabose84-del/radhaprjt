using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Domain.Common;

namespace SalesManagement.Application.CustomerVisit.Commands.DeleteCustomerVisitImage
{
    public class DeleteCustomerVisitImageCommandHandler : IRequestHandler<DeleteCustomerVisitImageCommand, bool>
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<DeleteCustomerVisitImageCommandHandler> _logger;

        public DeleteCustomerVisitImageCommandHandler(
            IFileUploadService fileUploadService,
            ILogger<DeleteCustomerVisitImageCommandHandler> logger)
        {
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteCustomerVisitImageCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.ImagePath))
                return false;

            // Construct full file path
            var fullPath = Path.Combine("Resources", MiscEnumEntity.CustomerVisit, request.ImagePath);

            var result = await _fileUploadService.DeleteFileAsync(fullPath);

            _logger.LogInformation("CustomerVisit image deleted: {ImagePath}, Success: {Result}", request.ImagePath, result);

            return result;
        }
    }
}
