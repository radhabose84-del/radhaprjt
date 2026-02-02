// using AutoMapper;
// using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
// using PurchaseManagement.Application.Quotation.RfqEntry.Dtos;
// using PurchaseManagement.Domain.Events;
// using MediatR;

// namespace PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetRfqAutoComplete;

// public class GetRfqAutoCompleteQueryHandler
//     : IRequestHandler<GetRfqAutoCompleteQuery, List<RfqAutoCompleteDto>>
// {
//     private readonly IRfqQueryRepository _repo;
//     private readonly IMapper _mapper;
//     private readonly IMediator _mediator;

//     public GetRfqAutoCompleteQueryHandler(IRfqQueryRepository repo, IMapper mapper, IMediator mediator)
//     {
//         _repo = repo;
//         _mapper = mapper;
//         _mediator = mediator;
//     }

//     public async Task<List<RfqAutoCompleteDto>> Handle(GetRfqAutoCompleteQuery request, CancellationToken cancellationToken)
//     {
//         var search = request.SearchPattern?.Trim();
//         var result = await _repo.GetRfqAutoCompleteAsync(search,request.LastSubmitDate, cancellationToken);

//         // If your repo already returns DTOs, you can skip _mapper.Map(...)
//         var items = _mapper.Map<List<RfqAutoCompleteDto>>(result);

//         // Domain event (same style as your quotation version)
//         var domainEvent = new AuditLogsDomainEvent(
//             actionDetail: "GetAll",
//             actionCode: "GetRfqAutoCompleteQuery",
//             actionName: items.Count.ToString(),
//             details: "RFQ autocomplete fetched.",
//             module: "RFQ"
//         );
//         await _mediator.Publish(domainEvent, cancellationToken);

//         return items;
//     }
    
// }
