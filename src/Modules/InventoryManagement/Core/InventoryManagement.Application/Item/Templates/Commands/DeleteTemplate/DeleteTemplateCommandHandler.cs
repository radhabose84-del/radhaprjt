using InventoryManagement.Application.Common.Interfaces.Item.Templates;
using MediatR;

namespace InventoryManagement.Application.Item.Templates.Commands.DeleteTemplate
{
    public sealed class DeleteTemplateCommandHandler : IRequestHandler<DeleteTemplateCommand, bool>
    {
        private readonly ITemplateCommandRepository _cmd;
        private readonly ITemplateQueryRepository _qry;

        public DeleteTemplateCommandHandler(ITemplateCommandRepository cmd, ITemplateQueryRepository qry)
        { _cmd = cmd; _qry = qry; }

        public async Task<bool> Handle(DeleteTemplateCommand request, CancellationToken ct)
        {
            var exists = await _qry.GetByIdAsync(request.Id, ct);
            if (exists is null) return false;

            await _cmd.SoftDeleteAsync(request.Id, ct);
            return true;
        }
    }
}
