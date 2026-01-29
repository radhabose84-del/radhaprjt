using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetLocation;
using FAM.Application.Common.Interfaces.ILocation;
using FAM.Application.Common.Interfaces.ISubLocation;
using FAM.Application.Location.Command.DeleteAubLocation;
using FAM.Application.SubLocation.Queries.GetSubLocations;
using FAM.Domain.Common;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.SubLocation.Command.DeleteSubLocation
{
    public class DeleteSubLocationCommandHandler : IRequestHandler<DeleteSubLocationCommand, bool>
    {
        private readonly ISubLocationCommandRepository _sublocationCommandRepository;
        private readonly ISubLocationQueryRepository _subLocationQueryRepository;
        private readonly ILocationQueryRepository _locationQueryRepository;
        private readonly IAssetLocationQueryRepository _assetLocationQueryRepository;

        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public DeleteSubLocationCommandHandler(ISubLocationCommandRepository sublocationCommandRepository, IAssetLocationQueryRepository assetLocationQueryRepository, IMediator mediator, IMapper mapper, ISubLocationQueryRepository subLocationQueryRepository, ILocationQueryRepository locationQueryRepository)
        {
            _sublocationCommandRepository = sublocationCommandRepository;
            _subLocationQueryRepository = subLocationQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
            _locationQueryRepository = locationQueryRepository;
            _assetLocationQueryRepository = assetLocationQueryRepository;
        }
        public async Task<bool> Handle(DeleteSubLocationCommand request, CancellationToken cancellationToken)
        {
            var sublocations = await _subLocationQueryRepository.GetByIdAsync(request.Id);
            if (sublocations is null || sublocations.IsDeleted is BaseEntity.IsDelete.Deleted)
            {
                throw new ValidationException("Invalid SubLocationID.The specified SubLocation does not exist or is inactive.");
             
            }
            
            var linked = await _subLocationQueryRepository.IsSubLocationLinkedAsync(request.Id);
            if (linked)
            {
            throw new ValidationException("This master is linked with other records. You cannot delete this record.");
            }
        
            var (assetSublocations, _) = await _assetLocationQueryRepository.GetAllAssetLocationAsync(1, 1, request.Id.ToString());
            if (assetSublocations.Any(a => a.SubLocationId == request.Id))
            {
                throw new ValidationException("SubLocation is in use by an Asset Location. Cannot delete.");
             
            }
            var sublocation = _mapper.Map<FAM.Domain.Entities.SubLocation>(request);
            var sublocationresult = await _sublocationCommandRepository.DeleteAsync(request.Id, sublocation);


            //Domain Event  
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: sublocation.Id.ToString(),
                actionName: sublocation.Id.ToString(),
                details: $"SubLocation '{sublocation.Id}' was deleted.",
                module: "SubLocation"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            if (sublocationresult)
            {
                return sublocationresult;
            }
            throw new Exception("SubLocation not deleted.");
            
        }
    }
}