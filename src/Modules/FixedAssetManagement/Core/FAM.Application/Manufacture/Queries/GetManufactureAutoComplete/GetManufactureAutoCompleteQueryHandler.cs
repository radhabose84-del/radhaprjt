using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IManufacture;
using FAM.Application.Manufacture.Queries.GetManufacture;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.Manufacture.Queries.GetManufactureAutoComplete
{
    public class GetManufactureAutoCompleteQueryHandler : IRequestHandler<GetManufactureAutoCompleteQuery, List<ManufactureAutoCompleteDTO>>
    {
        private readonly IManufactureQueryRepository _manufactureRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public GetManufactureAutoCompleteQueryHandler(IManufactureQueryRepository manufactureRepository,  IMapper mapper, IMediator mediator)
        {
            _manufactureRepository =manufactureRepository;
            _mapper =mapper;
            _mediator = mediator;
        }

        public async Task<List<ManufactureAutoCompleteDTO>> Handle(GetManufactureAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _manufactureRepository.GetByManufactureNameAsync(request.SearchPattern ?? string.Empty);
            if (result is null || result.Count is 0)
            {
                throw new ValidationException("No Manufacture found matching the search pattern.");
               
            }
            var manufacturesDto = _mapper.Map<List<ManufactureAutoCompleteDTO>>(result);
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAutoComplete",
                actionCode:"",        
                actionName: request.SearchPattern ?? string.Empty,                
                details: $"Manufacture '{request.SearchPattern}' was searched",
                module:"Manufacture"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return  manufacturesDto;
        }
    }
}