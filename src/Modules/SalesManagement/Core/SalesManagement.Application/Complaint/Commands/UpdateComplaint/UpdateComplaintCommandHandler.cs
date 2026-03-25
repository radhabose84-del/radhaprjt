using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Complaint.Commands.UpdateComplaint
{
    public class UpdateComplaintCommandHandler : IRequestHandler<UpdateComplaintCommand, ApiResponseDTO<int>>
    {
        private readonly IComplaintCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateComplaintCommandHandler(
            IComplaintCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateComplaintCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<ComplaintHeader>(request);

            // Map details
            var details = new List<ComplaintDetail>();
            if (request.Details != null && request.Details.Count > 0)
            {
                foreach (var detail in request.Details)
                {
                    var detailEntity = _mapper.Map<ComplaintDetail>(detail);
                    if (detail.NatureOfComplaintIds != null && detail.NatureOfComplaintIds.Count > 0)
                    {
                        detailEntity.ComplaintDetailNatures = detail.NatureOfComplaintIds
                            .Select(natureId => new ComplaintDetailNature
                            {
                                NatureOfComplaintId = natureId
                            }).ToList();
                    }
                    details.Add(detailEntity);
                }
            }

            var result = await _commandRepository.UpdateAsync(entity, details);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "COMPLAINT_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Complaint with Id {request.Id} updated successfully.",
                module: "Complaint");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Complaint updated successfully.",
                Data = result
            };
        }
    }
}
