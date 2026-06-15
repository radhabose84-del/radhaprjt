using FAM.Application.AssetMaster.AssetInsurance.Queries.GetAssetInsurance;
using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetMaster.AssetInsurance.Commands.CreateAssetInsurance
{
    public class CreateAssetInsuranceCommand : IRequest<GetAssetInsuranceDto>, IRequirePermission
    {        
        public int  AssetId { get; set; }       
        public string? PolicyNo { get; set; }       
        public DateOnly StartDate { get; set; }
        public int Insuranceperiod { get; set; }  
        public DateOnly EndDate { get; set; }
        public decimal PolicyAmount { get; set; }
        public string? VendorCode { get; set; }
        public int RenewalStatus { get; set; }
        public DateOnly RenewedDate { get; set; }
        public byte IsActive { get; set; }
        
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
