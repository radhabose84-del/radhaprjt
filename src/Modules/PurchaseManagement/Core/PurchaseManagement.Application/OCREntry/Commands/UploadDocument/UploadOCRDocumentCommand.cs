using MediatR;
using Microsoft.AspNetCore.Http;
using PurchaseManagement.Application.OCREntry.Dto;

namespace PurchaseManagement.Application.OCREntry.Commands.UploadDocument;

public class UploadOCRDocumentCommand : IRequest<UploadOCRDocumentResultDto>
{
    public IFormFile? File { get; set; }
}
