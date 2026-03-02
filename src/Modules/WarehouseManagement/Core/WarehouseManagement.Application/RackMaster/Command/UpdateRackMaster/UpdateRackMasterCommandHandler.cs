using System.ComponentModel.DataAnnotations;
using AutoMapper;
using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using WarehouseManagement.Domain.Events;
using MediatR;

namespace WarehouseManagement.Application.RackMaster.Command.UpdateRackMaster
{
    public class UpdateRackMasterCommandHandler : IRequestHandler<UpdateRackMasterCommand, int>
    {
        private readonly IRackMasterCommandRepository _rackMasterCommandRepository;
        private readonly IRackCodeGenerator _rackCodeGenerator;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateRackMasterCommandHandler(
            IRackMasterCommandRepository rackMasterCommandRepository,
            IMapper mapper,
            IMediator mediator,
            IRackCodeGenerator rackCodeGenerator)
        {
            _rackMasterCommandRepository = rackMasterCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
            _rackCodeGenerator = rackCodeGenerator;
        }

        public async Task<int> Handle(UpdateRackMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = await _rackMasterCommandRepository.GetByIdAsync(request.Id);
            if (entity is null)
            {
                throw new ValidationException($"RackMaster with Id {request.Id} not found.");
            }

            bool codeDriversChanged =
                entity.FloorId != request.FloorId ||
                entity.AisleId != request.AisleId ||
                entity.RackLevelId != request.RackLevelId;

            _mapper.Map(request, entity);

            if (codeDriversChanged &&
                entity.FloorId.HasValue &&
                entity.AisleId.HasValue &&
                entity.RackLevelId.HasValue)
            {
                entity.RackCode = await _rackCodeGenerator.GenerateAsync(
                    entity.WarehouseId, entity.FloorId, entity.AisleId, entity.RackLevelId);
            }

            entity.ModifiedDate = DateTimeOffset.UtcNow;

            await _rackMasterCommandRepository.UpdateAsync(entity);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "RACK_UPDATE",
                actionName: entity.RackCode ?? entity.RackName ?? string.Empty,
                details: $"RackMaster '{entity.RackCode ?? entity.RackName}' updated.",
                module: "RackMaster"), cancellationToken);

            return entity.Id;
        }
    }
}
