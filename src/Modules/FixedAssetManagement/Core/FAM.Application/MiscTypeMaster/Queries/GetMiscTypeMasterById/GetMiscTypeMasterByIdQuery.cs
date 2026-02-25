using FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById
{
    public class GetMiscTypeMasterByIdQuery :  IRequest<GetMiscTypeMasterDto>
    {

        public int Id { get; set; }
        
    }
}