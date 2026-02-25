using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeederGroup;
using MediatR;

namespace MaintenanceManagement.Application.Power.FeederGroup.Command.CreateFeederGroup
{
    public class CreateFeederGroupCommandHandler : IRequestHandler<CreateFeederGroupCommand, int>
    {
        private readonly IFeederGroupCommandRepository _feederGroupCommandRepository;
        private readonly IMapper _mapper;
        public CreateFeederGroupCommandHandler(IFeederGroupCommandRepository repository, IMapper mapper)
        {
            _feederGroupCommandRepository = repository;
            _mapper = mapper;
        }

         public async Task<int> Handle(CreateFeederGroupCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<MaintenanceManagement.Domain.Entities.Power.FeederGroup>(request);

        int newId = await _feederGroupCommandRepository.CreateAsync(entity);

        return newId > 0 ? newId : throw new ExceptionRules("FeederGroup Creation Failed.");
    }   

    }
}