using Core.Application.Common;
using Core.Application.Common.HttpResponse;
using Core.Application.Units.Queries.GetUnits;
using MediatR;


namespace Core.Application.Units.Queries.GetUnitByUserId
{
    public class GetUnitByUserIdQuery : IRequest<List<UnitAutoCompleteDTO>>
    {        
        public int CompanyId { get; set; }   
        public int UserId { get; set; }     
    }
}