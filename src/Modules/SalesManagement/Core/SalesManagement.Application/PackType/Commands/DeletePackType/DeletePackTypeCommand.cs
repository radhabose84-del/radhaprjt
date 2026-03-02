using MediatR;

namespace SalesManagement.Application.PackType.Commands.DeletePackType;

public sealed record DeletePackTypeCommand(int Id) : IRequest<bool>;
