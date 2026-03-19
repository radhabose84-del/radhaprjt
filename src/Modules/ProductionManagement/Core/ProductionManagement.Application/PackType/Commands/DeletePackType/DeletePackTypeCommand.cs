using MediatR;

namespace ProductionManagement.Application.PackType.Commands.DeletePackType;

public sealed record DeletePackTypeCommand(int Id) : IRequest<bool>;
