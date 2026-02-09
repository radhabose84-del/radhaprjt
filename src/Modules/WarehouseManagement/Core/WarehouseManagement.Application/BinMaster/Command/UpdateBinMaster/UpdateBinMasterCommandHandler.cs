using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using WarehouseManagement.Application.Common.Interfaces.IBinMaster;
using WarehouseManagement.Domain.Events;
using MediatR;

namespace WarehouseManagement.Application.BinMaster.Command.UpdateBinMaster
{
    public class UpdateBinMasterCommandHandler : IRequestHandler<UpdateBinMasterCommand, int>
    {

        private readonly IBinMasterCommandRepository _binRepo;

        private readonly IBinMasterQueryRepository _binMasterQueryRepository;
        private readonly IBinCodeGenerator _binCodeGenerator;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public UpdateBinMasterCommandHandler(IBinMasterCommandRepository binRepo, IMapper mapper, IMediator mediator, IBinMasterQueryRepository binMasterQueryRepository, IBinCodeGenerator binCodeGenerator)
        {
            _binRepo = binRepo;
            _mapper = mapper;
            _mediator = mediator;
            _binMasterQueryRepository = binMasterQueryRepository;
            _binCodeGenerator = binCodeGenerator;
        }

        public  async Task<int> Handle(UpdateBinMasterCommand request, CancellationToken cancellationToken)
        {
                            var entity = await _binRepo.GetByIdAsync(request.Id, cancellationToken);
                    if (entity is null)
                        throw new ValidationException($"BinMaster with Id {request.Id} not found.");

                    var oldCode = entity.BinCode;

                    // detect code-driver changes (add WarehouseId if you allow changing it)
                    bool codeDriversChanged = entity.RackId != request.RackId;

                    // map request -> domain entity (BinCode ignored in the profile)
                    _mapper.Map(request, entity);

                    if (codeDriversChanged)
                    {
                        entity.BinCode = await _binCodeGenerator.GenerateAsync(
                            entity.WarehouseId,
                            entity.RackId,
                            cancellationToken);
                    }
                 

                    await _binRepo.UpdateAsync(entity, cancellationToken );

                    await _mediator.Publish(new AuditLogsDomainEvent(
                        actionDetail: "Update",
                        actionCode:   "BIN_UPDATE",
                        actionName:   entity.BinCode ?? entity.BinName ?? string.Empty,
                        details:      codeDriversChanged
                                        ? $"BinMaster '{oldCode}' -> '{entity.BinCode}' updated. Id={entity.Id}"
                                        : $"BinMaster '{entity.BinCode}' updated. Id={entity.Id}",
                        module:       "BinMaster"), cancellationToken);

                    return entity.Id;
        }
    }
}