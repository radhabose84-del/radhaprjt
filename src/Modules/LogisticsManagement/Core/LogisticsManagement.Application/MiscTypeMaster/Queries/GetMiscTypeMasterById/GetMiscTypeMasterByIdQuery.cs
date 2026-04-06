using MediatR;
using LogisticsManagement.Application.MiscTypeMaster.Dto;

namespace LogisticsManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById
{
    public class GetMiscTypeMasterByIdQuery : IRequest<MiscTypeMasterDto?>
    {
        public int Id { get; set; }
    }
}
