using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IUnit;
using FluentValidation;

namespace UserManagement.Application.Units.Commands.DeleteUnit
{
    public class DeleteUnitCommandHandler : IRequestHandler<DeleteUnitCommand,int>
    {
          private readonly IUnitCommandRepository _iunitRepository;
          private readonly IUnitQueryRepository _IunitQueryRepository;
          private readonly IMapper _Imapper;
           private readonly ILogger<DeleteUnitCommandHandler> _logger;

        public DeleteUnitCommandHandler(IUnitCommandRepository iunitrepository,IMapper Imapper,ILogger<DeleteUnitCommandHandler> logger,IUnitQueryRepository IunitQueryRepository)
        {
            _iunitRepository = iunitrepository;
            _Imapper = Imapper;
            _logger = logger?? throw new ArgumentNullException(nameof(logger));
            _IunitQueryRepository = IunitQueryRepository;
        }

        public async Task<int> Handle(DeleteUnitCommand request, CancellationToken cancellationToken)
        {
              _logger.LogInformation($"Starting Deletion process for UnitId: {request.UnitId}");
              // 🔹 First, check if the ID exists in the database
            var existingunitId = await _IunitQueryRepository.GetByIdAsync(request.UnitId);
            if (existingunitId is null)
            {
                _logger.LogWarning($"Unit ID {request.UnitId} not found.");
                throw new ValidationException("Unit Id not found / Unit is deleted .");

            }
            
            var usedByUser  = await _IunitQueryRepository.IsUnitUsedByAnyUserAsync(request.UnitId);
            if (usedByUser)
            {
                throw new ValidationException("Cannot delete Unit : this record is referenced by other data.");
                // or: "Cannot delete Unit. Department already exists for this Unit."
            }

            var unit = _Imapper.Map<UserManagement.Domain.Entities.Unit>(request);
            var result = await _iunitRepository.DeleteUnitAsync(request.UnitId, unit);
            if (result == -1) // Unit not found
            {
                _logger.LogInformation($"Unit {request.UnitId} not found.");
                throw new ValidationException("Unit not found.");

            }
            _logger.LogInformation($"Completed Deletion process for UnitId: {request.UnitId}");
            var unitId = unit.Id;
            _logger.LogInformation($"Unit {unitId} deleted successfully", unitId);
            return unitId;

        }

    }
}