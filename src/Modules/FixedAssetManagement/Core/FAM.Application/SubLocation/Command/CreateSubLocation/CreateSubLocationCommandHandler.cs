#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.ISubLocation;
using FAM.Application.SubLocation.Queries.GetSubLocations;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;
using FAM.Domain.Common; 

namespace FAM.Application.SubLocation.Command.CreateSubLocation
{
    public class CreateSubLocationCommandHandler : IRequestHandler<CreateSubLocationCommand, SubLocationDto>
    {
         private readonly ISubLocationCommandRepository _sublocationCommandRepository;
        private readonly ISubLocationQueryRepository _sublocationQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public CreateSubLocationCommandHandler(ISubLocationCommandRepository sublocationCommandRepository,ISubLocationQueryRepository sublocationQueryRepository,IMapper mapper,IMediator mediator)
        {
            _sublocationCommandRepository = sublocationCommandRepository;
            _sublocationQueryRepository = sublocationQueryRepository;
            _mapper = mapper;
            _mediator = mediator;   
        }
        public async Task<SubLocationDto> Handle(CreateSubLocationCommand request, CancellationToken cancellationToken)
        {
            var isParentLocationActive = await _sublocationQueryRepository
                .IsParentLocationActiveAsync(request.LocationId);

            if (!isParentLocationActive)
            {
                throw new ValidationException("Cannot create Sub-location. Parent Location is inactive.");
            }

            var existingsubLocation = await _sublocationQueryRepository
                .GetBySubLocationNameAsync(request.SubLocationName, request.DepartmentId, request.LocationId, request.UnitId);

            if (existingsubLocation != null)
            {
                if (existingsubLocation.IsActive == BaseEntity.Status.Inactive)
                {
                    throw new ValidationException(
                        "A Sub-location with this Name already exists in inactive state. Please activate that record instead of creating a new one.");
                }

                throw new ValidationException("SubLocation already exists");
                   
               }

               // ✅ Check duplicate Code during CREATE
            var codeExists = await _sublocationCommandRepository.ExistsByCodeAsync(request.Code ?? string.Empty);

            if (codeExists)
            {
                throw new ValidationException("Sub Location Code already exists.");
            }

            var sublocation  = _mapper.Map<FAM.Domain.Entities.SubLocation>(request);

                var sublocationresult = await _sublocationCommandRepository.CreateAsync(sublocation);
                
                var sublocationMap = _mapper.Map<SubLocationDto>(sublocationresult);
                if (sublocationresult.Id > 0)
                {
                    var domainEvent = new AuditLogsDomainEvent(
                     actionDetail: "Create",
                     actionCode: sublocationresult.Code,
                     actionName: sublocationresult.SubLocationName,
                     details: $"SubLocation '{sublocationresult.Code}' was created. SubLocationName: {sublocationresult.SubLocationName}",
                     module:"SubLocation"
                 );
                 await _mediator.Publish(domainEvent, cancellationToken);
                 
                    return sublocationMap;
                }

               throw new Exception("SubLocation not created");
                    
        }
    }
}