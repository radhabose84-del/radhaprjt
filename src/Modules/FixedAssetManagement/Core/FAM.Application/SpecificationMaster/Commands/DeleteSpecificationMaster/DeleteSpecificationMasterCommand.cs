using FAM.Application.Common.HttpResponse;
using FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster;
using MediatR;

namespace FAM.Application.SpecificationMaster.Commands.DeleteSpecificationMaster
{
    public class DeleteSpecificationMasterCommand :  IRequest<SpecificationMasterDTO>
    {
         public int Id { get; set; }    
    }
}