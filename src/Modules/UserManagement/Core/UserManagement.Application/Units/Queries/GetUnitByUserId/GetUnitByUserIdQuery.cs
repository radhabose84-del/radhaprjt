using UserManagement.Application.Common;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Units.Queries.GetUnits;
using MediatR;


namespace UserManagement.Application.Units.Queries.GetUnitByUserId
{
    public class GetUnitByUserIdQuery : IRequest<List<UnitAutoCompleteDTO>>
    {        
        public int CompanyId { get; set; }   
        public int UserId { get; set; }     
    }
}