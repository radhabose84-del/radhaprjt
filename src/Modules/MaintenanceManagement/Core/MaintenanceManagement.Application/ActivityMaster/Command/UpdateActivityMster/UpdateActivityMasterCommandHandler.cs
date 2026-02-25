#nullable disable
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using FluentValidation;
using MediatR;

namespace MaintenanceManagement.Application.ActivityMaster.Command.UpdateActivityMster
{
    public class UpdateActivityMasterCommandHandler  : IRequestHandler<UpdateActivityMasterCommand, int>
    {
         private readonly IActivityMasterQueryRepository _activityMasterQueryRepository ;

          private readonly IActivityMasterCommandRepository _activityMasterCommandRepository ;
           private readonly IMapper _mapper;
          private readonly IMediator _mediator;

            private readonly IValidator<UpdateActivityMasterCommand> _validator;


        public UpdateActivityMasterCommandHandler(IActivityMasterQueryRepository activityMasterQueryRepository, IActivityMasterCommandRepository activityMasterCommandRepository, IMapper mapper, IMediator mediator , IValidator<UpdateActivityMasterCommand> validator)
        {
            _activityMasterQueryRepository = activityMasterQueryRepository;
            _activityMasterCommandRepository = activityMasterCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
            _validator = validator;
        }
         public async Task<int> Handle(UpdateActivityMasterCommand request, CancellationToken cancellationToken)
        { 
                        // 🔹 Retrieve Existing Record from Query Repository
                 // 🔹 Retrieve Existing Record from Query Repository
          //   var existingRecordDto = await _activityMasterQueryRepository.GetByIdAsync(request.UpdateActivityMaster.ActivityId);
            // if (existingRecordDto == null)
            // {
            //     return new ApiResponseDTO<int>
            //     {
            //         IsSuccess = false,
            //         Message = $"Activity Master with ID {request.UpdateActivityMaster.ActivityId} not found."
            //     };
            // }

            // 🔹 Convert DTO to Domain Entity
            var activityMasterEntity = _mapper.Map<MaintenanceManagement.Domain.Entities.ActivityMaster>(request.UpdateActivityMaster);

            if (request.UpdateActivityMaster.IsActive == 0)
            {
                var linked = await _activityMasterQueryRepository
                    .IsActivityMasterLinkedAsync(request.UpdateActivityMaster.ActivityId);

                if (linked)
                    throw new ValidationException(
                        "This master is linked with other records. You cannot inactivate this record.");
            }

            // 🔹 Save Changes
        //    var result = await _activityMasterCommandRepository.UpdateAsync(activityMasterEntity);
            var result = await _activityMasterCommandRepository.UpdateAsync(request.UpdateActivityMaster);
            
                return result > 0 ? result : throw new ExceptionRules("ActivityMaster update Failed.");
            
        }

        
    }
}