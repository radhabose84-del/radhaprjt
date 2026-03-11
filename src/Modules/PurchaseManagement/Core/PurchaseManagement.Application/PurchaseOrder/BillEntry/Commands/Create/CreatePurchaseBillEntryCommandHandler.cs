using AutoMapper;
using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IBillEntry;
using PurchaseManagement.Domain.Entities.PurchaseOrder.BillEntry;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.BillEntry.Commands.Create;

public sealed class CreatePurchaseBillEntryCommandHandler 
    : IRequestHandler<CreatePurchaseBillEntryCommand, int>
{
    private readonly IPurchaseBillEntryCommandRepository _repo;
    private readonly IPurchaseBillEntryQueryRepository _qryRepo;
    private readonly IMapper _mapper;
    private readonly IIPAddressService _ip;

    public CreatePurchaseBillEntryCommandHandler(
        IPurchaseBillEntryCommandRepository repo,
        IMapper mapper, IPurchaseBillEntryQueryRepository qryRepo, IIPAddressService ip)
    {
        _repo = repo;
        _mapper = mapper;
        _qryRepo = qryRepo;
        _ip=ip;
    }

    public async Task<int> Handle(CreatePurchaseBillEntryCommand request, CancellationToken ct)
    {
        var dto = request.Data;

        // unique bill check
        if (await _qryRepo.BillNumberExistsAsync(dto.PartyId, dto.BillNumber, null, ct))
            throw new InvalidOperationException("Bill number already exists for this vendor.");
        
        // map header + lines
        var header = _mapper.Map<PurchaseBillEntryHeader>(dto);
        header.Lines.Clear();

        foreach (var lineDto in dto.Lines)
        {
            var line = _mapper.Map<PurchaseBillEntryDetail>(lineDto);
            header.Lines.Add(line);
        }
        header.UnitId= _ip.GetUnitId() ?? 0;
        header.CreatedBy= _ip.GetUserId();
        header.CreatedByName= _ip.GetUserName();
        header.CreatedIP= _ip.GetUserIPAddress();
        header.CreatedDate= DateTime.Now;
        
        await _repo.AddAsync(header, ct);
        return header.Id;
    }  
}
