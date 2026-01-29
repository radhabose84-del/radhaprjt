using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.IDepartmentGroup;
using Core.Domain.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Application.DepartmentGroup.Command.CreateDepartmentGroup
{
    public class CreateDepartmentGroupCommandHandler : IRequestHandler<CreateDepartmentGroupCommand, int>
    {
        private readonly IDepartmentGroupCommandRepository _departmentGroupCommandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public CreateDepartmentGroupCommandHandler(
            IDepartmentGroupCommandRepository departmentGroupCommandRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _departmentGroupCommandRepository = departmentGroupCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        // ✅ This is the correct signature — NO explicit interface syntax
        public async Task<int> Handle(CreateDepartmentGroupCommand request, CancellationToken cancellationToken)
        {
            var departmentGroupEntity = _mapper.Map<Core.Domain.Entities.DepartmentGroup>(request);

            var result = await _departmentGroupCommandRepository.CreateAsync(departmentGroupEntity);

           if (result <= 0) 
            {
                throw new Exception("DepartmentGroup creation failed");
               
            }

            // Publish domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: departmentGroupEntity.DepartmentGroupCode.ToString(),
                actionName: departmentGroupEntity.DepartmentGroupName ?? "",
                details: $"Department Group '{departmentGroupEntity.DepartmentGroupName}' was created.",
                module: "DepartmentGroup");

            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
