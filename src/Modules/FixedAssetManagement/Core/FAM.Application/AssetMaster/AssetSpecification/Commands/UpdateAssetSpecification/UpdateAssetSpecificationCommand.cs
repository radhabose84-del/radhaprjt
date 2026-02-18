using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification;
using Contracts.Common;
using MediatR;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.AssetMaster.AssetSpecification.Commands.UpdateAssetSpecification
{
     public class UpdateAssetSpecificationCommand : IRequest<string>
    {
        public int AssetId { get; set; }
        public List<UpdateSpecificationItem>? Specifications { get; set; }
    }

    public class UpdateSpecificationItem
    {
        public int SpecificationId { get; set; }
        public string? SpecificationValue { get; set; }
        public byte IsActive  { get; set; }
    }
}