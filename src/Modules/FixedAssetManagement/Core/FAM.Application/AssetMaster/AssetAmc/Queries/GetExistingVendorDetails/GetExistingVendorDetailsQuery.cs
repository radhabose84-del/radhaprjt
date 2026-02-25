using MediatR;

namespace FAM.Application.AssetMaster.AssetAmc.Queries.GetExistingVendorDetails
{
    public class GetExistingVendorDetailsQuery : IRequest<List<GetExistingVendorDetailsDto>>
    {
        public string? OldUnitCode { get; set; }
        public string? VendorCode { get; set; }
       
    }
}