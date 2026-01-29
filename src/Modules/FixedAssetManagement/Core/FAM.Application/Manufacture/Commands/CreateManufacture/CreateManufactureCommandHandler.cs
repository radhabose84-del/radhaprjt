using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IManufacture;
using FAM.Application.Manufacture.Queries.GetManufacture;
using FAM.Domain.Entities;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.Manufacture.Commands.CreateManufacture
{
    public class CreateManufactureCommandHandler : IRequestHandler<CreateManufactureCommand, ManufactureDTO>
    {
        private readonly IMapper _mapper;
        private readonly IManufactureCommandRepository _manufactureRepository;
        private readonly IMediator _mediator;

        public CreateManufactureCommandHandler(IMapper mapper, IManufactureCommandRepository manufactureRepository, IMediator mediator)
        {
            _mapper = mapper;
            _manufactureRepository = manufactureRepository;
            _mediator = mediator;
        }

        public async Task<ManufactureDTO> Handle(CreateManufactureCommand request, CancellationToken cancellationToken)
        {
            // Validate duplicates BEFORE insert
            var codeExists = await _manufactureRepository.ExistsByCodeAsync(request.Code ?? string.Empty);
            var nameExists = await _manufactureRepository.ExistsByNameAsync(request.ManufactureName ?? string.Empty);

            if (codeExists && nameExists)
                throw new ValidationException("Manufacture Name and Manufacture Code already exists.");

            if (codeExists)
                throw new ValidationException("Manufacture Code already exists.");

            if (nameExists)
                throw new ValidationException("Manufacture Name already exists. Please use a different name.");

            // Create
            var manufactureEntity = _mapper.Map<Manufactures>(request);
            var result = await _manufactureRepository.CreateAsync(manufactureEntity);

            var manufactureDto = _mapper.Map<ManufactureDTO>(result);
            if (manufactureDto.Id > 0)
            {
                //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Create",
                    actionCode: manufactureEntity.Code ?? string.Empty,
                    actionName: manufactureEntity.ManufactureName ?? string.Empty,
                    details: $"Manufacture '{manufactureEntity.ManufactureName}' was created. Code: {manufactureEntity.Code}",
                    module: "Manufacture"
                );
                await _mediator.Publish(domainEvent, cancellationToken);

                return manufactureDto;
            }
            throw new Exception("Manufacture not created.");
        }
    }
}