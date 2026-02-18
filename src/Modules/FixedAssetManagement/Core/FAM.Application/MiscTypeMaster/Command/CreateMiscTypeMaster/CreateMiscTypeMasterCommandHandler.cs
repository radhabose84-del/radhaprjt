#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IMiscTypeMaster;
using FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.MiscTypeMaster.Command.CreateMiscTypeMaster
{
    public class CreateMiscTypeMasterCommandHandler : IRequestHandler<CreateMiscTypeMasterCommand, GetMiscTypeMasterDto>
    {
              private readonly IMiscTypeMasterCommandRepository _miscTypeMasterCommandRepository;
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;
        private readonly IMiscTypeMasterQueryRepository _miscTypeMasterQueryRepository;
     public CreateMiscTypeMasterCommandHandler (IMiscTypeMasterCommandRepository miscTypeMasterCommandRepository, IMapper imapper, IMediator mediator, IMiscTypeMasterQueryRepository miscTypeMasterQueryRepository)
        {
            _miscTypeMasterCommandRepository = miscTypeMasterCommandRepository;
            _imapper = imapper;
            _mediator = mediator;
            _miscTypeMasterQueryRepository = miscTypeMasterQueryRepository   ;
        }

        public async Task<GetMiscTypeMasterDto> Handle(CreateMiscTypeMasterCommand request, CancellationToken cancellationToken)
        {
               // 🔹 Check if a MiscTypeMaster with the same name already exists
            var existingMiscTypeMaster = await _miscTypeMasterQueryRepository.GetByMiscTypeMasterCodeAsync(request.MiscTypeCode);

            if (existingMiscTypeMaster != null)
            {
                throw new ValidationException("Misc Type Master already exists");
               
            }

            // 🔹 Map request to domain entity
            var miscTypeMaster = _imapper.Map<FAM.Domain.Entities.MiscTypeMaster>(request);

            // 🔹 Insert into the database

             var result = await _miscTypeMasterCommandRepository.CreateAsync(miscTypeMaster);
              if (result.Id <= 0)
                {
                    throw new Exception("Failed to create Misc Type Master");
               
            }

            // 🔹 Fetch newly created record
            var createdMiscTypeMaster = await _miscTypeMasterQueryRepository.GetByIdAsync(result.Id);
            var mappedResult = _imapper.Map<GetMiscTypeMasterDto>(createdMiscTypeMaster);

            // 🔹 Publish domain event for auditing/logging
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: miscTypeMaster.MiscTypeCode,
                actionName: miscTypeMaster.Description,
                details: $"Misc Type Master '{miscTypeMaster.MiscTypeCode}' was created.",
                module: "MiscTypeMaster"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            // 🔹 Return success response
            return  mappedResult;

        }
    }
}