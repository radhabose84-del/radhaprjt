using AutoMapper;
using Contracts.Interfaces;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Interfaces.IMRS;
using InventoryManagement.Domain.Entities.MRS;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.MRS.Command.UpdateMrsEntry
{
    public class UpdateMrsEntryCommandHandler : IRequestHandler<UpdateMrsEntryCommand, bool>
    {
        private readonly IMrsEntryCommandRepository _iMrsEntryCommandRepository;
        private readonly IMapper _mapper;
        
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;

        public UpdateMrsEntryCommandHandler(IMrsEntryCommandRepository iMrsEntryCommandRepository, IMapper mapper, IMediator mediator, IIPAddressService ipAddressService)
        {
            _iMrsEntryCommandRepository = iMrsEntryCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
        }

        public async Task<bool> Handle(UpdateMrsEntryCommand request, CancellationToken cancellationToken)
        {
         var mrsEntryHeader = _mapper.Map<MrsHeader>(request.updateMrsEntry);
            mrsEntryHeader.ModifiedBy = _ipAddressService.GetUserId();
            mrsEntryHeader.ModifiedDate = DateTime.Now;
            mrsEntryHeader.ModifiedByName = _ipAddressService.GetUserName();
            mrsEntryHeader.ModifiedIP = _ipAddressService.GetSystemIPAddress();
            // 🔹 Save Changes
            var result = await _iMrsEntryCommandRepository.UpdateAsync(mrsEntryHeader);
             //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: mrsEntryHeader.MrsNo ?? "NULL",
                actionName: mrsEntryHeader.MrsDate.ToString() ?? "NULL",
                details: $"MrsEntry Updated",
                module: "MrsEntry");
            await _mediator.Publish(domainEvent, cancellationToken);

            if (result)
            {
                return result;
            }
            throw new Exception("Mrs update failed");
            
        }
    }
}