using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using FAM.Application.Common.Interfaces.ISubLocation;
using FAM.Application.SubLocation.Queries.GetSubLocations;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.SubLocation.Queries.GetSubLocationById
{
    public class GetSubLocationByIdQueryHandler : IRequestHandler<GetSubLocationByIdQuery, SubLocationDto>
    {
        private readonly ISubLocationQueryRepository _sublocationQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;

        public GetSubLocationByIdQueryHandler(
            ISubLocationQueryRepository sublocationQueryRepository,
            IMapper mapper,
            IMediator mediator,
            IDepartmentLookup departmentLookup)
        {
            _sublocationQueryRepository = sublocationQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
        }
        public async Task<SubLocationDto> Handle(GetSubLocationByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _sublocationQueryRepository.GetByIdAsync(request.Id);
             if (result is null)
            {
                throw new ValidationException("SubLocationId not found");
               
            }  
           var sublocation = _mapper.Map<SubLocationDto>(result);

            // ✅ Enrich DepartmentName using lookup interface (UserManagement owner)
            if (sublocation.DepartmentId > 0)
            {
                var departments = await _departmentLookup.GetByIdsAsync(new[] { sublocation.DepartmentId }, cancellationToken);
                var dept = departments.FirstOrDefault();
                if (dept != null)
                {
                    sublocation.DepartmentName = dept.DepartmentName;
                }
            }

          //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: "",        
                    actionName: "",
                    details: $"SubLocation details {sublocation.Id} was fetched.",
                    module:"SubLocation"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
          return sublocation;
        }
    }
}