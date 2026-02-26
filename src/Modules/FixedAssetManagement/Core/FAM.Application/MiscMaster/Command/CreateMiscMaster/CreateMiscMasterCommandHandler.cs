using AutoMapper;
using FAM.Application.Common.Interfaces.IMiscMaster;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.MiscMaster.Command.CreateMiscMaster
{
    public class CreateMiscMasterCommandHandler  : IRequestHandler<CreateMiscMasterCommand, GetMiscMasterDto>
    {
       

        
        private readonly IMiscMasterCommandRepository _miscMasterCommandRepository;
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;

         public CreateMiscMasterCommandHandler (IMiscMasterCommandRepository miscMasterCommandRepository, IMapper imapper, IMediator mediator, IMiscMasterQueryRepository miscMasterQueryRepository)
        {
            _miscMasterCommandRepository = miscMasterCommandRepository;
            _imapper = imapper;
            _mediator = mediator;
            _miscMasterQueryRepository = miscMasterQueryRepository;
        }
        public  async Task<GetMiscMasterDto> Handle(CreateMiscMasterCommand request, CancellationToken cancellationToken)
        {
                // 🔹 Check if a MiscTypeMaster with the same name already exists
            var existingMiscMaster = await _miscMasterQueryRepository.GetByMiscMasterCodeAsync(request.Code,request.MiscTypeId) ;

            if (existingMiscMaster != null)
            {
                throw new ValidationException("Misc  Master already exists");
               
            }

            // 🔹 Map request to domain entity
            var miscMaster = _imapper.Map<FAM.Domain.Entities.MiscMaster>(request);

            // 🔹 Insert into the database

             var result = await _miscMasterCommandRepository.CreateAsync(miscMaster);
              if (result.Id <= 0)
                {
                    throw new ValidationException("Failed to create Misc  Master");
               
            }

            // 🔹 Fetch newly created record
            var createdMiscMaster = await _miscMasterQueryRepository.GetByIdAsync(result.Id);
            var mappedResult = _imapper.Map<GetMiscMasterDto>(createdMiscMaster);

            // 🔹 Publish domain event for auditing/logging
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: miscMaster.Code,
                actionName: miscMaster.Description,
                details: $"Misc  Master '{miscMaster.Code}' was created.",
                module: "MiscMaster"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            // 🔹 Return success response
            return mappedResult;




        }
    }
}