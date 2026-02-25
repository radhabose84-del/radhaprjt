using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using MediatR;
using ServiceEntryActivityEntity = PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO.ServiceEntryActivity;
using Contracts.Common;
using PurchaseManagement.Domain.Events;
using System.Text.Json;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.UpdateServiceEntrySheet
{
    public class UpdateServiceEntrySheetCommandHandler : IRequestHandler<UpdateServiceEntrySheetCommand, int>
    {
        private readonly IServicePurchaseOrderCommandRepository _servicePurchaseOrderCommandRepository;
        private readonly IServicePurchaseOrderQueryRepository _servicePurchaseOrderQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;

        public UpdateServiceEntrySheetCommandHandler(
           IServicePurchaseOrderCommandRepository servicePurchaseOrderCommandRepository,
           IServicePurchaseOrderQueryRepository poQuery,
           IMapper mapper,
           IMediator mediator,
          IMiscMasterQueryRepository miscTypeMasterQueryRepository)
        {
            _servicePurchaseOrderCommandRepository = servicePurchaseOrderCommandRepository;
            _servicePurchaseOrderQueryRepository = poQuery;
            _mapper = mapper;
            _mediator = mediator;
            _miscMasterQueryRepository = miscTypeMasterQueryRepository;
        }
        public async Task<int> Handle(UpdateServiceEntrySheetCommand request, CancellationToken ct)
        {


            var dto = request.Data ?? throw new ExceptionRules("Payload is required.");

            if (request.Id <= 0)
                throw new ExceptionRules("Valid Service Entry Sheet Id is required.");

            // keep dto in sync with command Id
            dto.Id = request.Id;

            // (A) Load existing SES
            var ses = await _servicePurchaseOrderCommandRepository
                .GetServiceEntrySheetByIdAsync(request.Id, ct);

            if (ses is null)
                throw new ExceptionRules($"Service Entry Sheet {request.Id} not found.");

                var approvedStatus = await _miscMasterQueryRepository.GetMiscMasterByName( MiscEnumEntity.ApprovalStatus,  MiscEnumEntity.Approved);
                if (ses.StatusId == approvedStatus.Id || ses.SESStatusId == approvedStatus.Id)
                    {
                        throw new ExceptionRules("Approved Service Entry Sheet cannot be modified.");
                    }
                    
            // (B) Validate PO/vendor (same as create logic)
            var po = await _servicePurchaseOrderQueryRepository
                .GetServicePOHeaderForSesAsync(dto.PurchaseOrderId, ct);

            if (po is null)
                throw new ExceptionRules($"PO {dto.PurchaseOrderId} not found or not a Service PO.");

            if (po.VendorId != dto.VendorId)
                throw new ExceptionRules(
                    $"Vendor mismatch. PO vendor={po.VendorId}, payload vendor={dto.VendorId}.");

            // (C) Map scalar fields from DTO → existing entity (without touching SES Id)
            _mapper.Map(dto, ses);

            // (D) ✅ Replace activities completely (no duplication)
            ses.Activities ??= new List<ServiceEntryActivityEntity>();

            // remove all existing activities – EF will handle delete if configured
            ses.Activities.Clear();

            if (dto.Activities is not null && dto.Activities.Count > 0)
            {
                foreach (var aDto in dto.Activities)
                {
                    var act = _mapper.Map<ServiceEntryActivityEntity>(aDto);

                    // ensure EF treats them as new children for this SES
                    act.Id = 0;                 // let DB generate new Id
                    // act.EntrySheetId = ses.Id; // optional if not handled by navigation

                    ses.Activities.Add(act);
                }
            }

             // (E) 📎 Documents: sync with UI (add new, keep existing, delete removed)
            ses.Documents ??= new List<ServiceEntrySheetDocument>();

            var existingDocs = ses.Documents.ToList();
            var incomingDtos = dto.ServiceEntryDocumentDtos ?? new List<CreateServiceSheetDto.ServiceEntryDocumentDto>();

            // Filter out dummy/invalid rows from UI
            var validIncoming = incomingDtos
                .Where(d => d.DocumentId != 0 &&
                            !string.Equals(d.FileName, "string", StringComparison.OrdinalIgnoreCase) &&
                            !string.IsNullOrWhiteSpace(d.FileName))
                .ToList();

            // 1) Remove documents that are not present anymore in the UI payload
            // build a case-insensitive key set: "DocumentId|FileName"
            var incomingKeySet = new HashSet<string>(
                validIncoming.Select(d => $"{d.DocumentId}|{d.FileName}"),
                StringComparer.OrdinalIgnoreCase);

            var toRemove = existingDocs
                .Where(ed => !incomingKeySet.Contains($"{ed.DocumentId}|{ed.FileName}"))
                .ToList();

            foreach (var rm in toRemove)
            {
                ses.Documents.Remove(rm);
            }

            // 2) Add new documents (and optionally handle newly uploaded temp files)
            if (validIncoming.Any())
            {
                var baseDirectory = MiscEnumEntity.DocumentPath; // e.g. "ServiceEntrySheet"
                var uploadPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Resources",
                    baseDirectory);

                EnsureDirectoryExists(uploadPath);

                foreach (var docDto in validIncoming)
                {
                    // If a document with same DocumentId + FileName already exists → keep it
                    var exists = existingDocs.FirstOrDefault(ed =>
                        ed.DocumentId == docDto.DocumentId &&
                        string.Equals(ed.FileName, docDto.FileName, StringComparison.OrdinalIgnoreCase));

                    if (exists != null)
                    {
                        // Optionally update UploadedDate / DocumentName here if needed
                        continue;
                    }

                    // New document: if FileName already looks like final (starts with "SES_"), don't rename
                    var isFinalName = docDto.FileName.StartsWith("SES_", StringComparison.OrdinalIgnoreCase);

                    var sourceFileName = docDto.FileName;
                    var sourcePath = Path.Combine(uploadPath, sourceFileName);

                    if (!File.Exists(sourcePath))
                    {
                        // If file is missing, skip this doc (or throw based on your requirement)
                        continue;
                    }

                    string finalFileName;

                    if (isFinalName)
                    {
                        finalFileName = sourceFileName;
                        // no rename needed
                    }
                    else
                    {
                        var sesDate = dto.SESDate == default
                            ? DateTimeOffset.UtcNow
                            : dto.SESDate;

                        var timeStamp = sesDate.ToString("yyyyMMdd'T'HHmmss");
                        finalFileName =
                            $"SES_{dto.PurchaseOrderId}_{dto.ScheduleID}_{dto.OccurrenceNo}_{docDto.DocumentId}_{timeStamp}{Path.GetExtension(sourcePath)}";

                        var finalPath = Path.Combine(uploadPath, finalFileName);

                        File.Move(sourcePath, finalPath, overwrite: true);
                    }

                    var uploadedDate = docDto.UploadedDate == default
                        ? DateTimeOffset.UtcNow
                        : docDto.UploadedDate;

                    var newDoc = new ServiceEntrySheetDocument
                    {
                        DocumentId   = docDto.DocumentId,
                        FileName     = finalFileName,
                        UploadedDate = uploadedDate,
                        UploadedPath = baseDirectory
                    };

                    ses.Documents.Add(newDoc);
                }
            }

            // (F) Recompute totals (same as create)
            decimal qty      = dto.ActualQuantity ?? 0m;
            decimal rate     = dto.ActualRate     ?? 0m;
            decimal baseVal  = dto.ActualValue    ?? decimal.Round(qty * rate, 2, MidpointRounding.AwayFromZero);

            decimal discount = dto.DiscountValue  ?? 0m;
            decimal taxable  = baseVal - discount;
            if (taxable < 0m) taxable = 0m;

            decimal taxPct   = dto.TaxPercentage  ?? 0m;
            decimal taxVal   = dto.TaxValue       ?? decimal.Round(taxable * (taxPct / 100m), 2, MidpointRounding.AwayFromZero);
            decimal totalVal = dto.TotalValue     ?? decimal.Round(taxable + taxVal, 2, MidpointRounding.AwayFromZero);

            ses.ActualValue = baseVal;
            ses.TaxValue    = taxVal;
            ses.TotalValue  = totalVal;

            // (G) Persist
            var saved = await _servicePurchaseOrderCommandRepository
                .UpdateServiceEntrySheetAsync(ses, ct);

            if (saved.Id <= 0)
                throw new ExceptionRules("Service Entry Sheet update failed.");

            // (H) Audit
            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "UpdateSES",
                actionName: "ServiceEntrySheet",
                details: JsonSerializer.Serialize(new { SESId = saved.Id, PO = saved.PurchaseOrderId }),
                module: "ServiceEntry"), ct);

            return saved.Id;
        }

      
         private static void EnsureDirectoryExists(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

    }
}