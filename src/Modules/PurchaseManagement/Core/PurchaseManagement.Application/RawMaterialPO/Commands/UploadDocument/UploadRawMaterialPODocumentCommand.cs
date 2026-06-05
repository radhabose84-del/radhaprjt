using MediatR;
using Microsoft.AspNetCore.Http;
using PurchaseManagement.Application.RawMaterialPO.Dto;

namespace PurchaseManagement.Application.RawMaterialPO.Commands.UploadDocument;

public class UploadRawMaterialPODocumentCommand : IRequest<UploadRawMaterialPODocumentResultDto>
{
    public IFormFile? File { get; set; }
}
