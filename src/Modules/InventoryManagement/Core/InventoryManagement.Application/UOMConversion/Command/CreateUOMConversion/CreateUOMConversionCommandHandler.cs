using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IUOMConversion;
using InventoryManagement.Application.UOMConversion.Queries.GetAllUOMConversion;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.UOMConversion.Command.CreateUOMConversion
{
    public class CreateUOMConversionCommandHandler : IRequestHandler<CreateUOMConversionCommand, ApiResponseDTO<UOMConversionDto>>
    {

        private readonly IUOMConversionCommandRepository _iUOMConversionCommandRepository;
        private readonly IUOMConversionQueryRepository _uOMConversionQueryRepository; 
        private readonly IMapper _imapper;
        private readonly  IMediator _mediator;

        public CreateUOMConversionCommandHandler(IUOMConversionCommandRepository iUOMConversionCommandRepository, IUOMConversionQueryRepository uOMConversionQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iUOMConversionCommandRepository = iUOMConversionCommandRepository;
            _uOMConversionQueryRepository = uOMConversionQueryRepository;
            _imapper = mapper;
            _mediator = mediator;
        } 
        
          public async Task<ApiResponseDTO<UOMConversionDto>> Handle(CreateUOMConversionCommand request, CancellationToken cancellationToken)
        {

            var uOMConversion = _imapper.Map<InventoryManagement.Domain.Entities.UOMConversion>(request);

            // 🔹 Insert into the database

            var result = await _iUOMConversionCommandRepository.CreateAsync(uOMConversion);


            // 🔹 Fetch newly created record
            var createdUOMConversion = await _uOMConversionQueryRepository.GetByIdAsync(result.Id);
            var mappedResult = _imapper.Map<UOMConversionDto>(createdUOMConversion);

            // 🔹 Publish domain event for auditing/logging
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode:  "uomconversion",
                actionName: "Create UOM Conversion",
                details: $"UOM Conversion '{createdUOMConversion.FromUOMId}' was created.",
                module: "UOM Conversion"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            // 🔹 Return success response
            return new ApiResponseDTO<UOMConversionDto>
            {
                IsSuccess = true,
                Message = "UOM Conversion created successfully",
                Data = mappedResult
            };


        }

    }
}