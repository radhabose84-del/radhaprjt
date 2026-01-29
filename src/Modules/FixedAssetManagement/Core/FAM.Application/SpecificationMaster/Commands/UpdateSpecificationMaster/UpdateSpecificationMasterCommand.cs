using FAM.Application.Common.HttpResponse;
using FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster;
using MediatR;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.SpecificationMaster.Commands.UpdateSpecificationMaster
{
    public class UpdateSpecificationMasterCommand  : IRequest<SpecificationMasterDTO>
    {
        public int Id { get; set; }       
        public string? SpecificationName { get; set; }
        public int? AssetGroupId { get; set; }
        public byte IsDefault { get; set; }               
        public byte IsActive { get; set; }
    }
}