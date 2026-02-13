using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;
using PurchaseManagement.Domain.Entities.ValueObjects;
using MediatR;

namespace PurchaseManagement.Application.Quotation.RfqEntry.Commands.Update
{
    public class UpdateRfqCommandHandler : IRequestHandler<UpdateRfqCommand, Unit>
    {
        private readonly IRfqCommandRepository _repo;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ip;

        public UpdateRfqCommandHandler(IRfqCommandRepository repo, IMapper mapper, IIPAddressService ip)
            => (_repo, _mapper, _ip) = (repo, mapper, ip);

        public async Task<Unit> Handle(UpdateRfqCommand request, CancellationToken ct)
        {
            // Trimmed, uppercase status code (no trailing spaces)
            var submitStatusId = await _repo.GetStatusIdByCodeAsync("SUBMIT", ct);

            var status = request.IsActive == 1
                ? BaseEntity.Status.Active
                : BaseEntity.Status.Inactive;

            // Header only (IDs/flags); UnitId from IP service
            var headerAfter = new RfqMaster
            {
                Id              = request.Id,
                InitiationTypeId= request.InitiationTypeId,
                IndentId        = request.IndentId,       
                UnitId          = _ip.GetUnitId(),
                RfqStatusId     = submitStatusId,
                IsActive        = status
            };

            // Build fresh line snapshots (we ignore incoming Ids during replace)
            var desiredItems = request.Items.Select(i => new RfqItem
            {
                // Id intentionally ignored (replace semantics)
                RfqId    = request.Id,
                ItemId   = i.ItemId,
                HsnId    = i.HsnId,
                Quantity = i.Quantity,
                UomId    = i.UomId
            }).ToList();

            var desiredSuppliers = request.Suppliers.Select(s =>
            {
                var sup = new RfqSupplier
                {
                    // Id intentionally ignored (replace semantics)
                    RfqId      = request.Id,
                    SupplierId = s.SupplierId,
                    Name       = s.Name,
                    Mobile     = s.Mobile,
                    GSTNumber  = s.Gst // map DTO "Gst" → entity "GSTNumber"
                };

                if (!string.IsNullOrWhiteSpace(s.Email))
                    sup.Email = new EmailAddress(s.Email!);

                return sup;
            }).ToList();

            await _repo.UpdateAsync(
                id: request.Id,
                headerAfter: headerAfter,
                desiredItems: desiredItems,
                desiredSuppliers: desiredSuppliers,
                ct: ct);

            return Unit.Value;
        }
    }
}
