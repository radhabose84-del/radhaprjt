// using System;
// using System.Collections.Generic;
// using System.ComponentModel.DataAnnotations;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
// using PurchaseManagement.Domain.Common;
// using PurchaseManagement.Domain.Entities.PurchaseOrder;
// using PurchaseManagement.Domain.PurchaseOrder;
// using MediatR;

// namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Update
// {
//     public class UpdateServicePoCommandHandler : IRequestHandler<UpdateServicePoCommand, bool>
//     {

//         private readonly IMapper _mapper;
//         private readonly IServicePurchaseOrderCommandRepository _repo;
//         private readonly IIPAddressService _ip;
//         private readonly ITimeZoneService _tz;

//         public UpdateServicePoCommandHandler(IMapper mapper, IServicePurchaseOrderCommandRepository repo, IIPAddressService ip, ITimeZoneService tz)
//         {
//             _mapper = mapper;
//             _repo = repo;
//             _ip = ip;
//             _tz = tz;
//         }
        
//         public async Task<bool> Handle(UpdateServicePoCommand request, CancellationToken cancellationToken)
//         {

//              var dto = request.Data ?? throw new ValidationException("Body is required.");
//         if (dto.Id <= 0)
//             throw new ValidationException("Service PO id is required.");
//             // Map the data from the request
//             var entity = _mapper.Map<PurchaseOrderHeader>(request.Data);
            
//             // Make sure the ID is set explicitly, either from the URL or request body
//           //  entity.Id = request.Id; // Make sure the ID is correctly set from the URL

//             // Handle audit data
//             entity.ModifiedBy = _ip.GetUserId();
//             entity.ModifiedByName = _ip.GetUserName();
//             entity.ModifiedIP = _ip.GetSystemIPAddress();

//             var tzi = _tz.GetSystemTimeZone();
//             var sysTz = TimeZoneInfo.FindSystemTimeZoneById(
//                 tzi.Equals("India Standard Time", StringComparison.OrdinalIgnoreCase)
//                     ? "Asia/Kolkata"
//                     : tzi);
//             entity.ModifiedDate = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, sysTz);
            
//            // 📎 Documents (rename + attach)
//             if (dto.Documents != null && dto.Documents.Count > 0)
//             {
//                 // same filter as create
//                 dto.Documents = dto.Documents
//                     .Where(d => d.DocumentId != 0 &&
//                                 !string.Equals(d.FileName, "string", StringComparison.OrdinalIgnoreCase))
//                     .ToList();

//                 if (dto.Documents.Any())
//                 {
//                     // use SAME base folder as create
//                     var baseDir   = MiscEnumEntity.DocumentPath; // e.g. "PoDocument"
//                     var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDir);
//                     EnsureDir(uploadDir);

//                     foreach (var doc in dto.Documents.Where(d => !string.IsNullOrWhiteSpace(d.FileName)))
//                     {
//                         var oldPath = Path.Combine(uploadDir, doc.FileName!);

//                         if (File.Exists(oldPath))
//                         {
//                             var finalName = $"{dto.PONumber}_{doc.DocumentId}{Path.GetExtension(oldPath)}";
//                             var newPath   = Path.Combine(uploadDir, finalName);

//                             if (!string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
//                             {
//                                 File.Move(oldPath, newPath, overwrite: true);
//                                 doc.FileName = finalName;
//                             }
//                         }

//                         if (doc.UploadedDate == default)
//                             doc.UploadedDate = DateTimeOffset.UtcNow;
//                     }

//                     // Attach to entity so EF updates PurchaseDocuments
//                     entity.PurchaseDocumentTypes = dto.Documents
//                         .Select(d => new PurchaseDocument
//                         {
//                             DocumentId   = d.DocumentId,
//                             FileName     = d.FileName,
//                             UploadedDate = d.UploadedDate
//                         })
//                         .ToList();
//                 }
//             }


//             // Call the repository to update the entity
//             return await _repo.UpdateAsync(entity, cancellationToken);
//         }
//           private static void EnsureDir(string path)
//             {
//                 if (!Directory.Exists(path))
//                     Directory.CreateDirectory(path);
//             }

       
//     }
// }