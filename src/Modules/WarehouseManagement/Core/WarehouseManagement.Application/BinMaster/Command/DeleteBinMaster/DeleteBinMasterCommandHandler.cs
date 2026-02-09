using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseManagement.Application.Common.Interfaces.IBinMaster;

using MediatR;

namespace WarehouseManagement.Application.BinMaster.Command.DeleteBinMaster
{
    public class DeleteBinMasterCommandHandler : IRequestHandler<DeleteBinMasterCommand, bool>
    {

        public readonly IBinMasterCommandRepository _binMasterCommandRepository;
        
        public readonly IBinMasterCommandRepository _rackMasterCommandRepository;
          
        public DeleteBinMasterCommandHandler(IBinMasterCommandRepository binMasterCommandRepository)
        {
            _binMasterCommandRepository = binMasterCommandRepository;
        }

        public async Task<bool> Handle(DeleteBinMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = await _binMasterCommandRepository.GetByIdAsync(request.Id, cancellationToken);
         if (entity is null || entity.IsDeleted == WarehouseManagement.Domain.Common.BaseEntity.IsDelete.Deleted)
            return false;

        var deletedId = await _binMasterCommandRepository.DeleteAsync(request.Id, cancellationToken);
        return deletedId > 0; 
        }
    }
}