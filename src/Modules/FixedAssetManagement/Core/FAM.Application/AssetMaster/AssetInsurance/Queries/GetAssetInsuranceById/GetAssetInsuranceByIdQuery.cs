using FAM.Application.AssetMaster.AssetInsurance.Queries.GetAssetInsurance;
using MediatR;

namespace FAM.Application.AssetMaster.AssetInsurance.Queries.GetAssetInsuranceById
{
    public class GetAssetInsuranceByIdQuery  : IRequest<GetAssetInsuranceDto>
    {
          public int Id { get; set; }
    }
}