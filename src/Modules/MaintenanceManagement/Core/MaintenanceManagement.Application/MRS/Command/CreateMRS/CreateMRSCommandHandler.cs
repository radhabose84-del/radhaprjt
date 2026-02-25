#nullable disable
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMRS;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MRS.Command.CreateMRS
{
    public class CreateMRSCommandHandler : IRequestHandler<CreateMRSCommand, int>
    {
        private readonly IMRSCommandRepository _imrscommandrepository;
        private readonly IMRSQueryRepository _imrsqueryrepository;
        private readonly IMediator _imediator;
        public CreateMRSCommandHandler(IMRSCommandRepository imrscommandrepository, IMediator imediator, IMRSQueryRepository imrsqueryrepository)
        {
            _imrscommandrepository = imrscommandrepository;
            _imediator = imediator;
            _imrsqueryrepository = imrsqueryrepository;
           
        }
        public async Task<int> Handle(CreateMRSCommand request, CancellationToken cancellationToken)
        {
                      
            var irno = await _imrscommandrepository.InsertMRSAsync(request.Header);
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: request.Header.IrDate.ToString(),
                actionName: request.Header.Refno.ToString(),
                details: $"MRS details was created",
                module: "MRS");
            await _imediator.Publish(domainEvent, cancellationToken);

            return  irno > 0 ? irno : throw new ExceptionRules("MRS Creation Failed.");
        }
    }
}