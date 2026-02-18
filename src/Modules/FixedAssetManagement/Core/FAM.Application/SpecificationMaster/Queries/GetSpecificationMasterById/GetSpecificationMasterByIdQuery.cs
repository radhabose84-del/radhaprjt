using Contracts.Common;
using FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster;
using MediatR;

namespace FAM.Application.SpecificationMaster.Queries.GetSpecificationMasterById
{
    public class GetSpecificationMasterByIdQuery : IRequest<SpecificationMasterDTO>
    {
        public int Id { get; set; }
    }
}