using AutoMapper;
using FAM.Application.Common.Interfaces.ILocation;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.Location.Command.UpdateLocation
{
    public class UpdateLocationCommandHandler : IRequestHandler<UpdateLocationCommand, bool>
    {
        private readonly ILocationCommandRepository _locationCommandRepository;
        private readonly ILocationQueryRepository _locationQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public UpdateLocationCommandHandler(ILocationCommandRepository locationCommandRepository, ILocationQueryRepository locationQueryRepository, IMediator mediator, IMapper mapper)
        {
            _locationCommandRepository = locationCommandRepository;
            _locationQueryRepository = locationQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }
        public async Task<bool> Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
        {
            // var existingLocation = await _locationQueryRepository.GetByLocationNameAsync(request.LocationName,request.DepartmentId,request.UnitId, request.Id);

            //     if (existingLocation != null)
            //     {
            //         return new ApiResponseDTO<bool>{IsSuccess = false, Message = "Location already exists"};
            //     }

            // Block inactivation if linked with SubLocations
            if (request.IsActive == 0) // 0 = Inactive
            {
                var isLinked = await _locationQueryRepository.IsLinkedWithSubLocationsAsync(request.Id);
                if (isLinked)
                    throw new ValidationException("This master is linked with other records. You cannot inactivate this record.");
            }

            // Check for duplicate GroupName or SortOrder
            var (isNameDuplicate, isSortOrderDuplicate) = await _locationCommandRepository
                                     .CheckForDuplicatesAsync(request.LocationName, request.SortOrder, request.Id);

            if (isNameDuplicate || isSortOrderDuplicate)
            {
                string errorMessage = isNameDuplicate && isSortOrderDuplicate
                ? "Both Location Name and Sort Order already exist."
                : isNameDuplicate
                ? "Location with the same LocationName already exists."
                : "Location with the same Sort Order already exists.";
            throw new ValidationException(errorMessage);
                
            }

            var location = _mapper.Map<FAM.Domain.Entities.Location>(request);

            var locationresult = await _locationCommandRepository.UpdateAsync(location);


            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: location.Code,
                actionName: location.LocationName,
                details: $"Location '{location.Id}' was updated.",
                module: "Location"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            if (locationresult)
            {
                return locationresult;
            }
            throw new Exception("Location not updated.");
            
        }
    }
}