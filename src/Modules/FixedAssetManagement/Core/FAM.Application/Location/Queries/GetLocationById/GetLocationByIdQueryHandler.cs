using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using FAM.Application.Common.Interfaces.ILocation;
using FAM.Application.Location.Queries.GetLocations;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.Location.Queries.GetLocationById
{
    public class GetLocationByIdQueryHandler : IRequestHandler<GetLocationByIdQuery, LocationDto>
    {
        private readonly ILocationQueryRepository _locationQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;

        public GetLocationByIdQueryHandler(
            ILocationQueryRepository locationQueryRepository,
            IMediator mediator,
            IMapper mapper,
            IDepartmentLookup departmentLookup)
        {
            _locationQueryRepository = locationQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
            _departmentLookup = departmentLookup;
        }
        public async Task<LocationDto> Handle(GetLocationByIdQuery request, CancellationToken cancellationToken)
        {
           var result = await _locationQueryRepository.GetByIdAsync(request.Id);
            if (result is null)
            {
                throw new ValidationException("LocationId not found");
                
            }  
           var location = _mapper.Map<LocationDto>(result);

            // ✅ Enrich DepartmentName using lookup interface (UserManagement owner)
            if (location.DepartmentId > 0)
            {
                var departments = await _departmentLookup.GetByIdsAsync(new[] { location.DepartmentId }, cancellationToken);
                var dept = departments.FirstOrDefault();
                if (dept != null)
                {
                    location.DepartmentName = dept.DepartmentName;
                }
            }

          //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: "",        
                    actionName: "",
                    details: $"Location details {location.Id} was fetched.",
                    module:"Location"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
          return location;
        }
    }
}
