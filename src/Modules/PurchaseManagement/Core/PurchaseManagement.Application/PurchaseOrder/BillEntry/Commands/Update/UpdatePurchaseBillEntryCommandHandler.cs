using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IBillEntry;
using PurchaseManagement.Domain.Entities.PurchaseOrder.BillEntry;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.BillEntry.Commands.Update;

public sealed class UpdatePurchaseBillEntryCommandHandler 
    : IRequestHandler<UpdatePurchaseBillEntryCommand, Unit>
{
    private readonly IPurchaseBillEntryCommandRepository _repo;
    private readonly IPurchaseBillEntryQueryRepository _qryRepo;
    private readonly IMapper _mapper;
    private readonly IIPAddressService _ip;

    public UpdatePurchaseBillEntryCommandHandler(
        IPurchaseBillEntryCommandRepository repo,
        IMapper mapper,
        IPurchaseBillEntryQueryRepository qryRepo, IIPAddressService ip)
    {
        _repo = repo;
        _mapper = mapper;
        _qryRepo = qryRepo;
        _ip=ip;
    }

    public async Task<Unit> Handle(UpdatePurchaseBillEntryCommand request, CancellationToken ct)
    {
        var dto = request.Data;

        var header = await _qryRepo.GetByIdAsync(dto.Id!.Value, ct);
        if (header is null)
            throw new KeyNotFoundException($"Bill entry {dto.Id} not found.");

        // Unique bill check (exclude self)
        if (await _qryRepo.BillNumberExistsAsync(dto.PartyId, dto.BillNumber, dto.Id, ct))
            throw new InvalidOperationException("Bill number already exists for this vendor.");

        // map scalar header fields
        _mapper.Map(dto, header);

        // rebuild lines (simpler)
        header.Lines.Clear();
        foreach (var lineDto in dto.Lines)
        {
            var line = _mapper.Map<PurchaseBillEntryDetail>(lineDto);
            header.Lines.Add(line);
        }
        header.UnitId= _ip.GetUnitId();
        header.ModifiedBy= _ip.GetUserId();
        header.ModifiedByName= _ip.GetUserName();
        header.ModifiedIP= _ip.GetUserIPAddress();
        header.ModifiedDate= DateTime.Now;

        await _repo.UpdateAsync(header, ct);
        return Unit.Value;
    }
}
