using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;
using System.Text.Json;

namespace SalesManagement.Application.Complaint.Commands.CreateComplaint
{
    public class CreateComplaintCommandHandler : IRequestHandler<CreateComplaintCommand, ApiResponseDTO<int>>
    {
        private readonly IComplaintCommandRepository _commandRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateComplaintCommandHandler(
            IComplaintCommandRepository commandRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IDocumentSequenceLookup documentSequenceLookup,
            IIPAddressService ipAddressService,
            IOutboxEventPublisher outboxEventPublisher,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _documentSequenceLookup = documentSequenceLookup;
            _ipAddressService = ipAddressService;
            _outboxEventPublisher = outboxEventPublisher;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateComplaintCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<ComplaintHeader>(request);

            // Set Pending status from ApprovalStatus
            var pendingStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.ComplaintApprovalStatus, MiscEnumEntity.ComplaintApprovalPending);
            entity.StatusId = pendingStatus?.Id ?? 0;

            // Map details
            if (request.Details != null && request.Details.Count > 0)
            {
                entity.ComplaintDetails = new List<ComplaintDetail>();
                foreach (var detail in request.Details)
                {
                    var detailEntity = _mapper.Map<ComplaintDetail>(detail);

                    // Map nature of complaint IDs to junction entities
                    if (detail.NatureOfComplaintIds != null && detail.NatureOfComplaintIds.Count > 0)
                    {
                        detailEntity.ComplaintDetailNatures = detail.NatureOfComplaintIds
                            .Select(natureId => new ComplaintDetailNature
                            {
                                NatureOfComplaintId = natureId
                            }).ToList();
                    }

                    entity.ComplaintDetails.Add(detailEntity);
                }
            }

            // Get UnitId from JWT token
            var unitId = _ipAddressService.GetUnitId();

            // Generate Complaint Number from Finance.DocumentSequence
            var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                "Complaint", "Sales", unitId ?? 0);
            if (!typeId.HasValue)
                throw new ExceptionRules("Transaction Type 'Complaint' not found for Sales module.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
            var complaintNumber = sequences.Count > 0 ? sequences[^1] : null;
            entity.ComplaintNumber = complaintNumber
                ?? throw new ExceptionRules("No document sequence configured for Complaint.");

            var newId = await _commandRepository.CreateAsync(entity, typeId.Value);

            // Publish workflow approval request via Outbox
            var correlationId = Guid.NewGuid();
            var payload = JsonSerializer.Serialize(new
            {
                Header = new
                {
                    Id = newId,
                    ComplaintNumber = complaintNumber,
                    CustomerId = request.CustomerId,
                    UnitId = unitId ?? 0,
                    StatusId = entity.StatusId
                },
                Lines = new List<object>()
            });

            var workflowEvent = new CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.ComplaintModuleTypeName,
                ModuleTransactionId = newId,
                Payload = payload
            };
            await _outboxEventPublisher.ScheduleAsync(workflowEvent, correlationId, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "COMPLAINT_CREATE",
                actionName: complaintNumber,
                details: $"Complaint '{complaintNumber}' created successfully with Id {newId}.",
                module: "Complaint");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Complaint created successfully.",
                Data = newId
            };
        }
    }
}
