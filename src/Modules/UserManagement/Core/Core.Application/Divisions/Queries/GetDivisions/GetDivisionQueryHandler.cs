using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.Interfaces;
using MediatR;
using System.Text;
using System.Data;
using Core.Application.Common.Interfaces.IDivision;
using Core.Application.Common.HttpResponse;
using Core.Domain.Events;

namespace Core.Application.Divisions.Queries.GetDivisions
{
    public class GetDivisionQueryHandler : IRequestHandler<GetDivisionQuery,ApiResponseDTO<List<DivisionDTO>>>
    {
        private readonly IDivisionQueryRepository _divisionRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetDivisionQueryHandler(IDivisionQueryRepository divisionRepository, IMapper mapper, IMediator mediator)
        {
            _divisionRepository = divisionRepository;
            _mapper =mapper;
            _mediator = mediator;
        }
        public async Task<ApiResponseDTO<List<DivisionDTO>>> Handle(GetDivisionQuery requst, CancellationToken cancellationToken)
        {
            var (divisions, totalCount) = await _divisionRepository.GetAllDivisionAsync(requst.PageNumber, requst.PageSize, requst.SearchTerm);
            var divisionList = _mapper.Map<List<DivisionDTO>>(divisions);

             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetDivisions",
                    actionCode: "",        
                    actionName: "",
                    details: $"Division details was fetched.",
                    module:"Division"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<DivisionDTO>> 
            { 
                IsSuccess = true, 
                Message = "Success", 
                Data = divisionList ,
                TotalCount = totalCount,
                PageNumber = requst.PageNumber,
                PageSize = requst.PageSize
                };
        }
    }
}