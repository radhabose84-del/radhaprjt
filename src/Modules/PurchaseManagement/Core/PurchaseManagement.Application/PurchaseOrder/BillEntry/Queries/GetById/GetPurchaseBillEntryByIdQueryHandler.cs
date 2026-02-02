using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IBillEntry;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Dto;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.BillEntry.Queries.GetById;

public sealed class GetPurchaseBillEntryByIdQueryHandler 
    : IRequestHandler<GetPurchaseBillEntryByIdQuery, PurchaseBillEntryHeaderDto>
{
    private readonly IPurchaseBillEntryQueryRepository _repo;
    private readonly IMapper _mapper;

    public GetPurchaseBillEntryByIdQueryHandler(
        IPurchaseBillEntryQueryRepository repo,
        IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<PurchaseBillEntryHeaderDto> Handle(
        GetPurchaseBillEntryByIdQuery request,
        CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            throw new KeyNotFoundException($"Bill entry {request.Id} not found.");

        return _mapper.Map<PurchaseBillEntryHeaderDto>(entity);
    }
}
