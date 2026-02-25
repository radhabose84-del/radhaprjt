using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using MediatR;

namespace WarehouseManagement.Application.RackMaster.Command.DeleteRackMaster
{
    public class DeleteRackMasterCommandHandler : IRequestHandler<DeleteRackMasterCommand, bool>
    {

         public readonly IRackMasterCommandRepository _rackMasterCommandRepository;

         public DeleteRackMasterCommandHandler( IRackMasterCommandRepository rackMasterCommandRepository)
        {
            _rackMasterCommandRepository = rackMasterCommandRepository;
        }

        public async Task<bool> Handle(DeleteRackMasterCommand request, CancellationToken cancellationToken)
        {
       
             var entity = await _rackMasterCommandRepository.GetByIdAsync(request.Id);
            if (entity is null || entity.IsDeleted == WarehouseManagement.Domain.Common.BaseEntity.IsDelete.Deleted)
                return false; 
           
            var deleted = await _rackMasterCommandRepository.DeleteAsync(request.Id, entity);
            return deleted;
       
        }
    }
}