#nullable disable
using AutoMapper;
using Contracts.Common;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.MiscMaster.Queries.GetMiscMaster;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.MiscMaster.Command.CreateMiscMaster
{
    public class CreateMiscMasterCommandHandler : IRequestHandler<CreateMiscMasterCommand, GetMiscMasterDto>
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
        public async Task<GetMiscMasterDto> Handle(CreateMiscMasterCommand request, CancellationToken cancellationToken)
        {

            var miscMaster = _imapper.Map<PurchaseManagement.Domain.Entities.MiscMaster>(request);

            // 🔹 Insert into the database

            var result = await _miscMasterCommandRepository.CreateAsync(miscMaster);


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
            return result.Id <= 0 ? throw new ExceptionRules("Failed to create Misc  Master") : mappedResult;



        }
    }
}