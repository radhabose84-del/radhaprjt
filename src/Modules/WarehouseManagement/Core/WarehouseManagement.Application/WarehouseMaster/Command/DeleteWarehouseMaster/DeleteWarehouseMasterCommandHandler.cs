using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using MediatR;

namespace WarehouseManagement.Application.WarehouseMaster.Command.DeleteWarehouseMaster
{
    public class DeleteWarehouseMasterCommandHandler : IRequestHandler<DeleteWarehouseMasterCommand, bool>
    {

        public readonly IWarehouseMasterCommandRepository _warehouseMasterCommandRepository;

      public DeleteWarehouseMasterCommandHandler( IWarehouseMasterCommandRepository warehouseMasterCommandRepository)
        {
            _warehouseMasterCommandRepository = warehouseMasterCommandRepository;
        }

        public async Task<bool> Handle(DeleteWarehouseMasterCommand request, CancellationToken cancellationToken)
        {
             var entity = await _warehouseMasterCommandRepository.GetByIdAsync(request.Id);
            if (entity is null || entity.IsDeleted == WarehouseManagement.Domain.Common.BaseEntity.IsDelete.Deleted)
                return false; 

           
            var deleted = await _warehouseMasterCommandRepository.DeleteAsync(request.Id, entity);
            return deleted;
        }
    }
}