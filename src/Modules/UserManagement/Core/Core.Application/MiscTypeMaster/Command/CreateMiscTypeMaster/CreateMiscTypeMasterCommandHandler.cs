using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.IMiscTypeMaster;
using Core.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using Core.Domain.Events;
using MediatR;

namespace Core.Application.MiscTypeMaster.Command.CreateMiscTypeMaster
{
    public class CreateMiscTypeMasterCommandHandler : IRequestHandler<CreateMiscTypeMasterCommand, ApiResponseDTO<GetMiscTypeMasterDto>>
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

        public async Task<ApiResponseDTO<GetMiscTypeMasterDto>> Handle(CreateMiscTypeMasterCommand request, CancellationToken cancellationToken)
        {
               // 🔹 Check if a MiscTypeMaster with the same name already exists
            // var existingMiscTypeMaster = await _miscTypeMasterQueryRepository.GetByMiscTypeMasterCodeAsync(request.MiscTypeCode);

            // if (existingMiscTypeMaster != null)
            // {
            //     return new ApiResponseDTO<GetMiscTypeMasterDto>
            //     {
            //         IsSuccess = false,
            //         Message = "Misc Type Master already exists",
            //         Data = null
            //     };
            // }

            // 🔹 Map request to domain entity
            var miscTypeMaster = _imapper.Map<Core.Domain.Entities.MiscTypeMaster>(request);

            // 🔹 Insert into the database

             var result = await _miscTypeMasterCommandRepository.CreateAsync(miscTypeMaster);
              if (result.Id <= 0)
                {
                return new ApiResponseDTO<GetMiscTypeMasterDto>
                {
                    IsSuccess = false,
                    Message = "Failed to create Misc Type Master",
                    Data = null
                };
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
            return new ApiResponseDTO<GetMiscTypeMasterDto>
            {
                IsSuccess = true,
                Message = "Misc Type Master created successfully",
                Data = mappedResult
            };

        }
    }
}