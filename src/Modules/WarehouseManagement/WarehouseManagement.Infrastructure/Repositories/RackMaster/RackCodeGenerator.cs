// using System;
// using System.CodeDom.Compiler;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.RegularExpressions;
// using System.Threading.Tasks;
// using Contracts.Interfaces.External.IInvetoryManagement;
// using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
// using Microsoft.EntityFrameworkCore;
// using WarehouseManagement.Infrastructure.Data;

// namespace WarehouseManagement.Infrastructure.Repositories.RackMaster
// {
//     public class RackCodeGenerator : IRackCodeGenerator
//     {

//         private readonly ApplicationDbContext _dbContext;
//         private readonly IMiscMasterGrpcClient _miscMasterGrpcClient;

//         public RackCodeGenerator(ApplicationDbContext dbContext, IMiscMasterGrpcClient miscMasterGrpcClient)
//         {
//             _dbContext = dbContext;
//             _miscMasterGrpcClient = miscMasterGrpcClient;
//         }
//         public async Task<string> GenerateAsync(int warehouseId, int? floorId, int? aisleId, int? rackLevelId)
//         {
//             var warehouseTypeId = await _dbContext.WarehouseMasters
//                 .Where(w => w.Id == warehouseId)
//                 .Select(w => w.WarehouseTypeId)
//                 .FirstOrDefaultAsync();
//             string wtCode = "WT";
//             if (warehouseTypeId > 0)
//             {
//                 var wtList = await _miscMasterGrpcClient.GetMiscMasterByIdAsync("WarehouseType");
//                 wtCode = wtList.FirstOrDefault(x => x.Id == warehouseTypeId)?.Code ?? "WT";
//             }

//             // 2) Floor/Aisle/Level codes (optional — if your ids point to MiscMaster)
//             string fCode = await GetMiscCodeAsync("Floor", floorId.Value);
//             string aCode = await GetMiscCodeAsync("WarehouseAisle", aisleId.Value);
//             string lCode = await GetMiscCodeAsync("WarehouseRackLevel", rackLevelId.Value);

//             // 3) Build prefix using codes (or keep your numeric formatting if you prefer)
//             string prefix = $"WH-{wtCode}-{fCode}-{aCode}-{lCode}-";

//            // 4) Find last code in that slot & increment
//                 var lastCode = await _dbContext.RackMasters.AsNoTracking()
//                     .Where(r => r.WarehouseId == warehouseId
//                             && r.FloorId     == floorId
//                             && r.AisleId     == aisleId
//                             && r.RackLevelId == rackLevelId)
//                           //  && r.IsDeleted   == WarehouseManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted)
//                     .OrderByDescending(r => r.Id)
//                     .Select(r => r.RackCode)
//                     .FirstOrDefaultAsync();

//                 var next = 1;
//                 if (!string.IsNullOrWhiteSpace(lastCode))
//                 {
//                     var m = Regex.Match(lastCode, @"-(\d{1,10})$");
//                     if (m.Success && int.TryParse(m.Groups[1].Value, out var n)) next = n + 1;
//                 }

//                 return $"{prefix}{next:D4}";
           
//         }
//           private async Task<string> GetMiscCodeAsync(string miscType, int id)
//         {
//             var items = await _miscMasterGrpcClient.GetMiscMasterByIdAsync(miscType);
//             var code  = items.FirstOrDefault(x => x.Id == id)?.Code;

//             if (string.IsNullOrWhiteSpace(code))
//                 throw new InvalidOperationException($"{miscType} misc code not found for Id {id}.");

//             return code;
//         }
        
       
//     }
// }