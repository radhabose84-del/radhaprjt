#nullable disable
using MediatR;

namespace SalesManagement.Application.SalesOrganisation.Commands.DeleteSalesOrganisation;

public sealed record DeleteSalesOrganisationCommand(int Id) : IRequest<bool>;
