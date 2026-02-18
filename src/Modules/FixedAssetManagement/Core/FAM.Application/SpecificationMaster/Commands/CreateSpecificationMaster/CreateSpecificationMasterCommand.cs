using Contracts.Common;
using FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster;
using MediatR;

namespace FAM.Application.SpecificationMaster.Commands.CreateSpecificationMaster
{
    public class CreateSpecificationMasterCommand : IRequest<SpecificationMasterDTO>
    {
        public string? SpecificationName { get; set; }      
        public int? AssetGroupId { get; set; } 
        public byte ISDefault { get; set; }       

    }
}