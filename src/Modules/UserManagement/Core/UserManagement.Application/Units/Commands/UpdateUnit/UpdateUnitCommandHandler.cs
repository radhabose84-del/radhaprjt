#nullable disable
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IUnit;
using FluentValidation;

namespace UserManagement.Application.Units.Commands.UpdateUnit
{
    public class UpdateUnitCommandHandler : IRequestHandler<UpdateUnitCommand, int>
    {
        private readonly IUnitCommandRepository _iUnitRepository;

        private readonly IUnitQueryRepository _iunitQueryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateUnitCommandHandler> _logger;

       public UpdateUnitCommandHandler(IUnitCommandRepository iUnitRepository, IMapper mapper, ILogger<UpdateUnitCommandHandler> logger, IUnitQueryRepository IunitQueryRepository)
        {
            _iUnitRepository = iUnitRepository;
            _mapper = mapper;
            _logger = logger?? throw new ArgumentNullException(nameof(logger));
            _iunitQueryRepository = IunitQueryRepository;
        }

        public async Task<int> Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting update process for UnitId: {request.UpdateUnitDto.Id}");

            // Inactivate guard — MUST run BEFORE persisting the update
            if (request.UpdateUnitDto.IsActive == 0)
            {
                var linked = await _iunitQueryRepository.IsUnitUsedByAnyUserAsync(request.UpdateUnitDto.Id);
                if (linked)
                    throw new ValidationException("This master is linked with other records. You cannot inactivate this record.");
            }

            //  First, check if the ID exists in the database
            var existingUnit = await _iunitQueryRepository.GetByIdAsync(request.UpdateUnitDto.Id);
            if (existingUnit is null )
            {
                // Idempotent update: a missing / already-deleted id is a no-op → controller 200, not 400.
                _logger.LogWarning($"Unit ID {request.UpdateUnitDto.Id} not found.");
                return 0;
            }

            // Check if unit name already exists for another ID
            var existingUnitName = await _iUnitRepository.ExistsByNameupdateAsync(request.UpdateUnitDto.UnitName,request.UpdateUnitDto.Id);
            if (existingUnitName)
            {
                _logger.LogWarning($"Unit name {request.UpdateUnitDto.UnitName} already exists.");
                throw new ValidationException("Unit name already exists.");
              
            }

            var unit = _mapper.Map<UserManagement.Domain.Entities.Unit>(request.UpdateUnitDto);
            var result =await _iUnitRepository.UpdateUnitAsync(request.UpdateUnitDto.Id, unit);
            if (result == -1)
            {
                 _logger.LogWarning($"UnitId not found: {request.UpdateUnitDto.Id}");

                    throw new ValidationException("UnitId not found");
                   
           
            }
            _logger.LogInformation($"Completed update process for UnitId: {request.UpdateUnitDto.Id}");

              var unitId = unit.Id;
              _logger.LogInformation($"Unit {unitId} Fetched successfully For Other Tables UnitAddress and UnitContacts");

              _logger.LogInformation($"Unit {unitId} Updated successfully");
              return unitId;

        
        }

       
    }
}