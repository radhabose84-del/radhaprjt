using UserManagement.Application.Common;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Units.Queries.GetUnits;
using MediatR;

namespace UserManagement.Application.Units.Queries.GetUnitById
{
    public class GetUnitByIdQuery :  IRequest<GetUnitsByIdDto>
    { 
        public int Id { get; set; }
    }
    
}