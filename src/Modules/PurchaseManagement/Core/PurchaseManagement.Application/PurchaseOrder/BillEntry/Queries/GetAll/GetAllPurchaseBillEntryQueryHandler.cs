using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IBillEntry;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Dto;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.BillEntry.Queries.GetAll;

public sealed class GetPurchaseBillEntryListQueryHandler 
    : IRequestHandler<GetAllPurchaseBillEntryQuery, PurchaseBillEntryListVm>
{
    private readonly IPurchaseBillEntryQueryRepository _repo;
    private readonly IMapper _mapper;

    public GetPurchaseBillEntryListQueryHandler(
        IPurchaseBillEntryQueryRepository repo,
        IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<PurchaseBillEntryListVm> Handle(
        GetAllPurchaseBillEntryQuery request,
        CancellationToken ct)
    {
        var (rows, total) = await _repo.GetListAsync(
            request.VendorId,
            request.Search,
            request.FromDate,
            request.ToDate,
            request.Page,
            request.Size,
            ct);

        var items = _mapper.Map<List<PurchaseBillEntryHeaderDto>>(rows);

        // for list, we normally don’t need lines -> clear to avoid heavy payload
       /*  foreach (var i in items) 
            i.Lines.Clear(); */

        return new PurchaseBillEntryListVm
        {
            Total = total,
            Items = items
        };
    }
}   
