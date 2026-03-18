using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Create;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.PurchaseOrder;
using MediatR;
using Microsoft.Extensions.Logging;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Update
{
    public class UpdateServicePoCommandHandler : IRequestHandler<UpdateServicePoCommand, bool>
    {

        private readonly IMapper _mapper;
        private readonly IServicePurchaseOrderCommandRepository _repo;
        private readonly IServicePurchaseOrderQueryRepository _queryRepo;
        private readonly IIPAddressService _ip;
        private readonly ITimeZoneService _tz;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly ILogger<UpdateServicePoCommandHandler> _logger;

        public UpdateServicePoCommandHandler(
            IMapper mapper,
            IServicePurchaseOrderCommandRepository repo,
            IServicePurchaseOrderQueryRepository queryRepo,
            IIPAddressService ip,
            ITimeZoneService tz,
            IOutboxEventPublisher outboxEventPublisher,
            ILogger<UpdateServicePoCommandHandler> logger)
        {
            _mapper = mapper;
            _repo = repo;
            _queryRepo = queryRepo;
            _ip = ip;
            _tz = tz;
            _outboxEventPublisher = outboxEventPublisher;
            _logger = logger;
        }

        public async Task<bool> Handle(UpdateServicePoCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Data ?? throw new ValidationException("Body is required.");
            if (dto.Id <= 0)
                throw new ValidationException("Service PO id is required.");

            // Map the data from the request
            var entity = _mapper.Map<PurchaseOrderHeader>(request.Data);

            // Handle audit data
            entity.ModifiedBy = _ip.GetUserId();
            entity.ModifiedByName = _ip.GetUserName();
            entity.ModifiedIP = _ip.GetSystemIPAddress();

            var tzi = _tz.GetSystemTimeZone();
            TimeZoneInfo sysTz;
            try
            {
                sysTz = TimeZoneInfo.FindSystemTimeZoneById(
                    tzi.Equals("India Standard Time", StringComparison.OrdinalIgnoreCase)
                        ? "Asia/Kolkata"
                        : tzi);
            }
            catch
            {
                sysTz = TimeZoneInfo.Local;
            }
            entity.ModifiedDate = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, sysTz);

            // Documents (rename + attach)
            if (dto.Documents != null && dto.Documents.Count > 0)
            {
                dto.Documents = dto.Documents
                    .Where(d => d.DocumentId != 0 &&
                                !string.Equals(d.FileName, "string", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (dto.Documents.Any())
                {
                    var baseDir = MiscEnumEntity.DocumentPath;
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDir);
                    EnsureDir(uploadDir);

                    foreach (var doc in dto.Documents.Where(d => !string.IsNullOrWhiteSpace(d.FileName)))
                    {
                        var oldPath = Path.Combine(uploadDir, doc.FileName!);

                        if (File.Exists(oldPath))
                        {
                            var finalName = $"{dto.PONumber}_{doc.DocumentId}{Path.GetExtension(oldPath)}";
                            var newPath = Path.Combine(uploadDir, finalName);

                            if (!string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
                            {
                                File.Move(oldPath, newPath, overwrite: true);
                                doc.FileName = finalName;
                            }
                        }

                        if (doc.UploadedDate == default)
                            doc.UploadedDate = DateTimeOffset.UtcNow;
                    }

                    entity.PurchaseDocumentTypes = dto.Documents
                        .Select(d => new PurchaseDocument
                        {
                            DocumentId = d.DocumentId,
                            FileName = d.FileName,
                            UploadedDate = d.UploadedDate
                        })
                        .ToList();
                }
            }

            // Persist update
            var result = await _repo.UpdateAsync(entity, cancellationToken);
            if (!result) return false;

            // Reload aggregate for workflow payload
            var agg = await _queryRepo.GetByIdAsync(dto.Id, cancellationToken)
                    ?? throw new InvalidOperationException($"Service PO {dto.Id} not found after update.");

            // Build workflow payload
            var wf = _mapper.Map<CreatePOServiceReverseDto>(agg);
            wf.Lines = new();

            var payload = JsonSerializer.Serialize(wf);
            var correlationId = Guid.NewGuid();

            // Schedule Outbox Event (SQL Transactional Outbox)
            var @event = new CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.ServicePO,
                ModuleTransactionId = dto.Id,
                Payload = payload
            };

            await _outboxEventPublisher.ScheduleAsync(@event, correlationId, cancellationToken);

            _logger.LogInformation(
                "ServicePO updated. Id={PoId}, CorrelationId={CorrelationId}",
                dto.Id, correlationId);

            return true;
        }

        private static void EnsureDir(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

    }
}
