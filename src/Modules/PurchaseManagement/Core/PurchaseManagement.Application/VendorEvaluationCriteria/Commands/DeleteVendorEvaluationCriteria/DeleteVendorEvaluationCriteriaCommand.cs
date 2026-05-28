using MediatR;

namespace PurchaseManagement.Application.VendorEvaluationCriteria.Commands.DeleteVendorEvaluationCriteria;

public sealed record DeleteVendorEvaluationCriteriaCommand(int Id) : IRequest<bool>;
