using AutoMapper;
using WarehouseManagement.Application.Common.Interfaces.IBinMaster;
using WarehouseManagement.Domain.Events;
using MediatR;

namespace WarehouseManagement.Application.BinMaster.Command.CreateBinMaster
{
    public class CreateBinMasterCommandHandler : IRequestHandler<CreateBinMasterCommand, int>
    {

        private readonly IBinMasterCommandRepository _binMasterCommandRepository;
        private readonly IBinCodeGenerator _binCodeGenerator; 
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public CreateBinMasterCommandHandler(IBinMasterCommandRepository binMasterCommandRepository, IMapper mapper, IMediator mediator, IBinCodeGenerator binCodeGenerator)
        {
            _binMasterCommandRepository = binMasterCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
            _binCodeGenerator = binCodeGenerator;
        }
      
        
         public async Task<int> Handle(CreateBinMasterCommand request, CancellationToken cancellationToken)    
        {                          
            

             var binCode = await _binCodeGenerator.GenerateAsync(request.WarehouseId, request.RackId, cancellationToken);
            if (string.IsNullOrWhiteSpace(binCode))
            {
                // safety fallback
                binCode = $"BIN-{request.WarehouseId}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
            }
            if (binCode.Length > 20) binCode = binCode[..20];

            // Normalize name
            var binName = (request.BinName ?? string.Empty).Trim();
            if (binName.Length > 50) binName = binName[..50];


            var entity = _mapper.Map<WarehouseManagement.Domain.Entities.BinMaster>(request);
            entity.BinCode = binCode;
            entity.BinName = binName;

            // Ensure scale matches config (decimal(18,3))
            entity.BinCapacity = Math.Round(request.BinCapacity, 3, MidpointRounding.AwayFromZero);

            // 3) Persist
            var newId = await _binMasterCommandRepository.CreateAsync(entity);

            // 4) Audit
            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode:   "BIN_CREATE",
                actionName:   entity.BinCode,
                details:      $"BinMaster '{entity.BinCode}' created successfully with Id {newId} (WarehouseId={entity.WarehouseId}, RackId={entity.RackId}).",
                module:       "BinMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            // 5) Return Id
            return newId;

        }
    }
}