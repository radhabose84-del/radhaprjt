using AutoMapper;
using FAM.Application.Common.Interfaces.ISubLocation;
using FAM.Application.Location.Command.UpdateSubLocation;
using FAM.Domain.Common;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.SubLocation.Command.UpdateSubLocation
{
    public class UpdateSubLocationCommandHandler : IRequestHandler<UpdateSubLocationCommand, bool>
    {
        private readonly ISubLocationCommandRepository _sublocationCommandRepository;
        private readonly ISubLocationQueryRepository _sublocationQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public UpdateSubLocationCommandHandler(ISubLocationCommandRepository sublocationCommandRepository, ISubLocationQueryRepository sublocationQueryRepository, IMapper mapper, IMediator mediator)
        {
            _sublocationCommandRepository = sublocationCommandRepository;
            _sublocationQueryRepository = sublocationQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<bool> Handle(UpdateSubLocationCommand request, CancellationToken cancellationToken)
        {
            var existingSubLocation = await _sublocationQueryRepository.GetByIdAsync(request.Id);


            var oldSubLocationName = existingSubLocation.SubLocationName;
            existingSubLocation.SubLocationName = request.SubLocationName;

            if (existingSubLocation is null || existingSubLocation.IsDeleted is BaseEntity.IsDelete.Deleted)
            {
                throw new ValidationException("Invalid SubLocationID. The specified SubLocationName does not exist or is deleted.");
              
            }

            if (request.IsActive == 0)
            {
                var linked = await _sublocationQueryRepository.IsSubLocationLinkedAsync(request.Id);
                if (linked)
                    throw new ValidationException("This master is linked with other records. You cannot inactivate this record.");
            }


            // Block activating / moving under inactive parent
            if (request.IsActive == 1)
            {
           var isParentLocationActive = await _sublocationQueryRepository.IsParentLocationActiveAsync(request.LocationId);
                if (!isParentLocationActive)
                {
                    throw new ValidationException("Cannot activate Sub-location. Parent Location is inactive."); 
                }
            }

            var sublocationExists = await _sublocationCommandRepository.ExistsByCodeAsync(request.Code ?? string.Empty, request.Id);

            if (sublocationExists)
            {
                throw new ValidationException("SubLocation Code already exists.");
              
            }

            var sublocation = _mapper.Map<FAM.Domain.Entities.SubLocation>(request);

            var sublocationresult = await _sublocationCommandRepository.UpdateAsync(sublocation);


            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: sublocation.Code ?? string.Empty,
                actionName: sublocation.SubLocationName ?? string.Empty,
                details: $"SubLocation '{oldSubLocationName}' was updated to {request.SubLocationName}'.  Code: {request.Code}.",
                module: "SubLocation"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            if (sublocationresult)
            {
                return sublocationresult;
            }
            throw new Exception("SubLocation not updated.");
            
        }
    }
}