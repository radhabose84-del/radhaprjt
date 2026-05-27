using MediatR;

namespace PurchaseManagement.Application.VendorEvaluationHeader.Commands.DeleteVendorEvaluationHeader;

public sealed record DeleteVendorEvaluationHeaderCommand(int Id) : IRequest<bool>;
