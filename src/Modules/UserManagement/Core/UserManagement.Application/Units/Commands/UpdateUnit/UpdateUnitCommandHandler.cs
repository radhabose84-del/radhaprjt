using UserManagement.Domain.Entities;
using UserManagement.Application.Common.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IUnit;
using UserManagement.Application.Common.HttpResponse;
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
            //  First, check if the ID exists in the database
            var existingUnit = await _iunitQueryRepository.GetByIdAsync(request.UpdateUnitDto.Id);
            if (existingUnit is null )
            {
                _logger.LogWarning($"Unit ID {request.UpdateUnitDto.Id} not found.");
                throw new ValidationException("Unit Id not found / Unit is deleted.");
                
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