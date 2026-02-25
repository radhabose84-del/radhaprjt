using MediatR;

namespace FAM.Application.AssetMaster.AssetInsurance.Commands.DeleteAssetInsurance
{
    public class DeleteAssetInsuranceCommand :  IRequest<bool>
    {
        public int Id { get; set; }  
    }
}