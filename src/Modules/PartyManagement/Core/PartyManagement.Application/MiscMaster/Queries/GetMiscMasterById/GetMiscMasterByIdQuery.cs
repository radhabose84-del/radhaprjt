using PartyManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace PartyManagement.Application.MiscMaster.Queries.GetMiscMasterById
{
    public class GetMiscMasterByIdQuery :  IRequest<GetMiscMasterDto>
    {
        public int Id { get; set; }
    }
}