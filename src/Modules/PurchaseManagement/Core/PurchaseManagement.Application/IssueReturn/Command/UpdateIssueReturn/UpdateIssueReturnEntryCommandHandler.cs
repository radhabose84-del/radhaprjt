using AutoMapper;
using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IIssueReturn;
using PurchaseManagement.Domain.Entities.IssueReturn;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.IssueReturn.Command.UpdateIssueReturn
{
    public class UpdateIssueReturnEntryCommandHandler : IRequestHandler<UpdateIssueReturnEntryCommand, bool>
    {
        private readonly IIssueReturnEntryCommandRepository _issueReturnEntryCommandRepository;
        private readonly IMapper _mapper;

        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;

        public UpdateIssueReturnEntryCommandHandler(IIssueReturnEntryCommandRepository issueReturnEntryCommandRepository, IMapper mapper, IMediator mediator, IIPAddressService ipAddressService)
        {
            _issueReturnEntryCommandRepository = issueReturnEntryCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
        }

        public async Task<bool> Handle(UpdateIssueReturnEntryCommand request, CancellationToken cancellationToken)
    {
        var issueReturnHeader = _mapper.Map<IssueReturnHeader>(request.updateIssueReturnEntry);

        // 🧩 Fill audit metadata
        issueReturnHeader.CreatedBy = _ipAddressService.GetUserId();
        issueReturnHeader.CreatedByName = _ipAddressService.GetUserName();
        issueReturnHeader.CreatedIP = _ipAddressService.GetSystemIPAddress();
        issueReturnHeader.CreatedDate = DateTime.Now;

        // 🔹 Update in repository
        var result = await _issueReturnEntryCommandRepository.UpdateAsync(issueReturnHeader);

        // 🪶 Domain Event
        var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "Update",
            actionCode: issueReturnHeader.IssueReturnNo ?? "NULL",
            actionName: issueReturnHeader.IssueReturnDate.ToString("yyyy-MM-dd") ?? "NULL",
            details: "Issue Return entry updated successfully.",
            module: "IssueReturn");

        await _mediator.Publish(domainEvent, cancellationToken);

        if (!result)
            throw new Exception("Issue Return update failed");

        return true;
    }
    }
}