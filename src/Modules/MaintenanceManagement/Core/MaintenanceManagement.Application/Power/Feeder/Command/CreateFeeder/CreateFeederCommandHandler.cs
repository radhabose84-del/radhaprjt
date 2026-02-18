using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeeder;
using MediatR;

namespace MaintenanceManagement.Application.Power.Feeder.Command.CreateFeeder
{
    public class CreateFeederCommandHandler   : IRequestHandler<CreateFeederCommand, int>
    {

        private readonly IFeederCommandRepository _feederCommandRepository;
        private readonly IMapper _mapper;

        public CreateFeederCommandHandler(IFeederCommandRepository feederCommandRepository, IMapper mapper)
        {
            _feederCommandRepository = feederCommandRepository;
            _mapper = mapper;
        }
           public async Task<int> Handle(CreateFeederCommand request, CancellationToken cancellationToken)
       {
            var entity = _mapper.Map<MaintenanceManagement.Domain.Entities.Power.Feeder>(request);        

            int newId = await _feederCommandRepository.CreateAsync(entity);

            return newId > 0 ? newId : throw new ExceptionRules("Feeder Creation Failed.");
       }   


    }
}