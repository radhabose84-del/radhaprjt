using MediatR;

namespace PurchaseManagement.Application.VendorRatingGrade.Commands.DeleteVendorRatingGrade;

public sealed record DeleteVendorRatingGradeCommand(int Id) : IRequest<bool>;
