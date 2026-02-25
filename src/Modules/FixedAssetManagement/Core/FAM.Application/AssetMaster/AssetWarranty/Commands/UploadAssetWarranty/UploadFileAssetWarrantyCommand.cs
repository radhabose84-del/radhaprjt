using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FAM.Application.AssetMaster.AssetWarranty.Commands.UploadAssetWarranty
{
    public class UploadFileAssetWarrantyCommand : IRequest<AssetWarrantyDTO>
    {
        public IFormFile? File { get; set; }
        // public string? CompanyName { get; set; }  
        // public string? UnitName { get; set; }  
        public string? AssetCode { get; set; }  
    }
}
