using MediatR;

namespace FAM.Application.AssetMaster.AssetSpecification.Commands.CreateAssetSpecification
{
    public class CreateAssetSpecificationCommand : IRequest<string>
    {
        public int AssetId { get; set; }
        public List<SpecificationItem>? Specifications { get; set; }
    }

    public class SpecificationItem
    {
        public int SpecificationId { get; set; }
        public string? SpecificationName { get; set; }
        public string? SpecificationValue { get; set; }        
    }
}