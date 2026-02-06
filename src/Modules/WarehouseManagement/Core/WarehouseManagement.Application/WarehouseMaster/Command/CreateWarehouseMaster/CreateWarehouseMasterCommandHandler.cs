// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using System.Data;
// using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
// using WarehouseManagement.Application.WarehouseMaster.GetAllWarehouseMaster;
// using MediatR;
// using Microsoft.EntityFrameworkCore;
// using AutoMapper;
// using WarehouseManagement.Domain.Entities;
// using Contracts.Interfaces.External.IInvetoryManagement;
// using Contracts.Interfaces.External.IWareHouse;
// using Microsoft.Extensions.Logging;


// namespace WarehouseManagement.Application.WarehouseMaster.Command.CreateWarehouseMaster
// {
//     public class CreateWarehouseMasterCommandHandler : IRequestHandler<CreateWarehouseMasterCommand, int>
//     {

//         private readonly IWarehouseCodeGenerator _warehouseCodeGenerator;
//         private readonly IWarehouseMasterCommandRepository _warehouseMasterCommandRepository;
//         private readonly IMapper _mapper;

//         private readonly IItemGroupGrpcClient  _itemGroupGrpcClient;

//         private readonly ILocationGrpcClient _locationGrpcClient;
        
//         private readonly ILogger<CreateWarehouseMasterCommandHandler> _log;



//         public CreateWarehouseMasterCommandHandler(IWarehouseCodeGenerator warehouseCodeGenerator, IWarehouseMasterCommandRepository warehouseMasterCommandRepository, IMapper mapper, IItemGroupGrpcClient itemGroupGrpcClient, ILocationGrpcClient locationGrpcClient, ILogger<CreateWarehouseMasterCommandHandler> log)
//         {
//             _warehouseCodeGenerator = warehouseCodeGenerator;
//             _warehouseMasterCommandRepository = warehouseMasterCommandRepository;
//             _mapper = mapper;
//             _itemGroupGrpcClient = itemGroupGrpcClient;
//             _log = log;
//             _locationGrpcClient = locationGrpcClient;
//         }
//         public async Task<int> Handle(CreateWarehouseMasterCommand request, CancellationToken cancellationToken)
//         {
            
//                     // Sanity guard so we never NRE at the gRPC call.
//                 if (_locationGrpcClient is null)
//                     throw new InvalidOperationException("ILocationGrpcClient is not registered. Check DI wiring.");

//                 _log.LogInformation(
//                     "WH Create: City='{City}', State='{State}', Country='{Country}', CityId={CityId}, StateId={StateId}, CountryId={CountryId}",
//                     request.City, request.State, request.Country, request.CityId, request.StateId, request.CountryId);               

               
//                     _log.LogInformation(
//                         "WH Create: City='{City}', State='{State}', Country='{Country}', CityId={CityId}, StateId={StateId}, CountryId={CountryId}",
//                         request.City, request.State, request.Country, request.CityId, request.StateId, request.CountryId);

//                  // ✅ Declare needResolve only once
//             var needResolve = request.CityId <= 0 || request.StateId <= 0 || request.CountryId <= 0;
//             _log.LogInformation("WH Create: needResolve={NeedResolve}", needResolve);

//             if (needResolve)
//                 {
//                     if (string.IsNullOrWhiteSpace(request.City) ||
//                         string.IsNullOrWhiteSpace(request.State) ||
//                         string.IsNullOrWhiteSpace(request.Country))
//                     {
//                         throw new InvalidOperationException(
//                             "Provide either CityId/StateId/CountryId or City/State/Country to create a warehouse.");
//                     }

//                     _log.LogInformation("Calling GetOrCreateLocation: {City}, {State}, {Country}", request.City, request.State, request.Country);
//                     // 👇 Your missing call — now in place
//                     var ids = await _locationGrpcClient.GetOrCreateLocationAsync(
//                         request.City.Trim(), request.State.Trim(), request.Country.Trim());

//                     if (ids is null)
//                         throw new InvalidOperationException("Unable to resolve City/State/Country.");



//                     request.CityId = ids.CityId;
//                     request.StateId = ids.StateId;
//                     request.CountryId = ids.CountryId;
                    

//                      _log.LogInformation("Resolved IDs: city={CityId}, state={StateId}, country={CountryId}",
//                     request.CityId, request.StateId, request.CountryId);
//                 }


//             var warehouseCode = await _warehouseCodeGenerator.GenerateAsync(
//                 request.UnitId, 
//                 request.WarehouseTypeId
//             );

           
//             var warehouse = _mapper.Map<WarehouseManagement.Domain.Entities.WarehouseMaster>(request);
//             warehouse.WarehouseCode = warehouseCode;

            
            

//               if (request.AllowedItemGroupIds == null || !request.AllowedItemGroupIds.Any())
//             {
//                 var itemGroups = await _itemGroupGrpcClient.GetAllItemGroupsAsync();
//                 request.AllowedItemGroupIds = itemGroups
//                     .Where(g => g.IsActive) // Only active ones
//                     .Select(g => g.Id)
//                     .ToList();
//             }

           
//             foreach (var itemGroupId in request.AllowedItemGroupIds)
//             {
//                 warehouse.AllowedItemGroups.Add(new WarehouseItemGroupMapping
//                 {
//                     ItemGroupId = itemGroupId,
//                     IsActive = WarehouseManagement.Domain.Common.BaseEntity.Status.Active,
//                     IsDeleted = WarehouseManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted

//                 });
//             }
           
//             var newId = await _warehouseMasterCommandRepository.CreateAsync(warehouse);
//             return newId;
//         }
       
        
//     }
// }