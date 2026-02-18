using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecificationById
{
    public class GetAssetSpecificationByIdQuery : IRequest<AssetSpecificationJsonDto>
    {
         public int Id { get; set; }
    }
}