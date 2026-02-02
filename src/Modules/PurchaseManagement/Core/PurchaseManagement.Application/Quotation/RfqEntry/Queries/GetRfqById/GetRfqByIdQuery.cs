using PurchaseManagement.Application.Quotation.RfqEntry.DTOs;
using MediatR;

namespace PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetRfqById;
public record GetRfqByIdQuery(int Id) : IRequest<RfqDto>;
