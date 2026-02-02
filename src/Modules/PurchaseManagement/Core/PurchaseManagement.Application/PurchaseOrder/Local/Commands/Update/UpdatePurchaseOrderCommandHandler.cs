// using System.ComponentModel.DataAnnotations;
// using AutoMapper;
// using Contracts.Interfaces.External.IBudget;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
// using PurchaseManagement.Domain.Entities.PurchaseOrder;
// using MediatR;

// namespace PurchaseManagement.Application.PurchaseOrder.Local.Commands.Update;

// public class UpdatePurchaseOrderCommandHandler : IRequestHandler<UpdatePurchaseOrderCommand, bool>
// {
//     private readonly IPurchaseOrderCommandRepository _repo;
//     private readonly IMapper _mapper;
//     private readonly IPODocumentQueryRepository _poDocs;
//     private readonly IBudgetAllocationGrpcClient _budgetAllocationGrpcClient;  

//       public UpdatePurchaseOrderCommandHandler(
//         IPurchaseOrderCommandRepository repo,
//         IMapper mapper,
//         IPODocumentQueryRepository poDocs,
//         IBudgetAllocationGrpcClient budgetAllocationGrpcClient)             
//     {
//         _repo = repo;
//         _mapper = mapper;
//         _poDocs = poDocs;
//         _budgetAllocationGrpcClient = budgetAllocationGrpcClient 
//             ?? throw new System.ArgumentNullException(nameof(budgetAllocationGrpcClient));
//     }

//     public async Task<bool> Handle(UpdatePurchaseOrderCommand request, CancellationToken ct)
//     {
//         var dto = request.Data ?? throw new ValidationException("Body required.");
//         if (dto.Id <= 0) throw new ValidationException("Purchase Order id is required.");

//         // -------------------------------------------------
//         // 🔍 BUDGET VALIDATION (BudgetGroup vs PurchaseValue)
//         // -------------------------------------------------
//         if (dto.BudgetGroupId.HasValue && dto.BudgetGroupId.Value > 0)
//         {
//             // Use PO date as budget date
//             DateOnly? budgetDate = null;
//             if (dto.PODate != default)
//             {
//                 budgetDate = DateOnly.FromDateTime(dto.PODate.DateTime);
//             }

//             var remaining = await _budgetAllocationGrpcClient.GetRemainingBalanceAsync(
//                 budgetGroupId: dto.BudgetGroupId.Value,
//                 budgetDate: budgetDate??DateOnly.FromDateTime(DateTime.UtcNow),
//                 monthId: dto.BudgetMonthId??0,
//                 requestById: dto.BudgetRequestById??0,
//                 projectId: dto.ProjectId,
//                 wbsId: dto.WBSId,financialYearId: dto.FinancialYearId,
//                 ct: ct);

//             var currentRemaining = remaining?.CurrentRemainingBalance ?? 0m;

//             if (dto.PurchaseValue > currentRemaining)
//             {
//                 // ❌ Same style as create, but for update
//                 throw new ValidationException("Cannot update PO. Budget amount less than PO value");
//             }
//         }

//         // Map header + children
//         var incoming = _mapper.Map<PurchaseOrderHeader>(dto);
//         incoming.Id = dto.Id;    
    
//         if (dto.Documents != null && dto.Documents.Count > 0)
//         {
//             var baseDir = "PoDocument";//await _poDocs.GetBaseDirectoryAsync();
//             var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDir);
//             if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

//             foreach (var doc in dto.Documents.Where(d => !string.IsNullOrWhiteSpace(d.FileName)))
//             {
//                 var oldPath = Path.Combine(uploadDir, doc.FileName!);
//                 if (File.Exists(oldPath))
//                 {
//                     var finalName = $"{dto.PONumber}_{doc.DocumentId}{Path.GetExtension(oldPath)}";
//                     var newPath = Path.Combine(uploadDir, finalName);
//                     if (!string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
//                     {
//                         File.Move(oldPath, newPath, overwrite: true);
//                         doc.FileName = finalName;
//                     }
//                 }

//                 if (doc.UploadedDate == default)
//                     doc.UploadedDate = DateTimeOffset.UtcNow;
//             }
//         }

//     // 🚫 No PODocumentsUpdate, no existingDocIds, no merging—use dto.Documents as-is
//     var updatedId = await _repo.UpdateAsync(incoming, dto, ct);
//     return updatedId > 0;
// }


//     private static void EnsureDir(string p)
//     {
//         if (!Directory.Exists(p)) Directory.CreateDirectory(p);
//     }
// }
