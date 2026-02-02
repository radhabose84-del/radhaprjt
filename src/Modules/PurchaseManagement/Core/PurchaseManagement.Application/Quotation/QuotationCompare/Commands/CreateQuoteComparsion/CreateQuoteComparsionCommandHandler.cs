// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.Json;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Events.Workflow;
// using PurchaseManagement.Application.Common.Exceptions;
// using PurchaseManagement.Application.Common.Interfaces;
// using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationCompare;
// using PurchaseManagement.Application.Quotation.QuotationCompare.Commands.CreateQuoteComparision;
// using PurchaseManagement.Domain.Common;
// using PurchaseManagement.Domain.Entities.Quotation.QuotationCompare;
// using PurchaseManagement.Domain.Events;
// using MediatR;

// namespace PurchaseManagement.Application.Quotation.QuotationCompare.Commands.CreateQuoteComparsion
// {
//     public class CreateQuoteComparsionCommandHandler  : IRequestHandler<CreateQuoteComparsionCommand, int>
//     {
//         private readonly IQuotationCompareCommandRepository _iquotationCompareCommandRepository;
//         private readonly IMediator _imediator;
//         private readonly IMapper _imapper;
//         private readonly IEventPublisher _eventPublisher;

//         public CreateQuoteComparsionCommandHandler(IQuotationCompareCommandRepository quotationCompareCommandRepository, IMediator mediator, IMapper mapper, IEventPublisher eventPublisher)
//         {
//             _iquotationCompareCommandRepository = quotationCompareCommandRepository;
//             _imediator = mediator;
//             _imapper = mapper;
//             _eventPublisher = eventPublisher;
//         }   

//          public async Task<int> Handle(CreateQuoteComparsionCommand request, CancellationToken cancellationToken)
//         {
//             // Map DTO → Domain Entity
//             var comparisonHeader = _imapper.Map<QuotationComparisonHeader>(request.CreateQuoteComparsion);


//             // Save to repository
//             var result = await _iquotationCompareCommandRepository.AddAsync(comparisonHeader);

//             var entity = await _iquotationCompareCommandRepository.GetByIdQuoteComparisonWorkFlowAsync(result);
//             var reverseMap = _imapper.Map<CreateQuoteComparisonReverseDto>(entity);
//             string serializedPayload = JsonSerializer.Serialize(reverseMap);

//             //Approval flow
//             if (result > 0)
//             {
//                 var correlationId = Guid.NewGuid();
//                 var @event = new TransactionCreatedEvent
//                 {
//                     CorrelationId = correlationId,
//                     ModuleTypeName = MiscEnumEntity.QuotationComparison,
//                     ModuleTransactionId = result,
//                     Payload = serializedPayload
//                 };

//                 await _eventPublisher.SaveEventAsync(@event);
//                 await _eventPublisher.PublishPendingEventsAsync();
//             }

//             // Example Domain Event (optional, adjust to your Audit log design)
//             var domainEvent = new AuditLogsDomainEvent(
//                 actionDetail: "Create",
//                 actionCode: comparisonHeader.RfqCode ?? "NULL",
//                 actionName: $"Comparison Created for RFQ {comparisonHeader.RfqId}",
//                 details: $"Quotation Comparison created with {comparisonHeader.QuotationConfirmedDetails.Count} detail(s).",
//                 module: "QuotationComparison");

//             await _imediator.Publish(domainEvent, cancellationToken);

//             return result > 0
//                 ? result
//                 : throw new ExceptionRules("Quotation Comparison creation failed.");
//         }
//     }
// }