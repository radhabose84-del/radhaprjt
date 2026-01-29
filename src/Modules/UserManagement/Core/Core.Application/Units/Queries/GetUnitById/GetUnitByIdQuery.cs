using Core.Application.Common;
using Core.Application.Common.HttpResponse;
using Core.Application.Units.Queries.GetUnits;
using MediatR;

namespace Core.Application.Units.Queries.GetUnitById
{
    public class GetUnitByIdQuery :  IRequest<GetUnitsByIdDto>
    { 
        public int Id { get; set; }
    }
    
}