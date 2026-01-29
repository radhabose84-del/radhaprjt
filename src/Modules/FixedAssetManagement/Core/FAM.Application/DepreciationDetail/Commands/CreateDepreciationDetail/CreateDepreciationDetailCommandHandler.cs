using AutoMapper;
using FAM.Application.Common.Exceptions;
using FAM.Application.Common.Interfaces.IDepreciationDetail;
using MediatR;

namespace FAM.Application.DepreciationDetail.Commands.CreateDepreciationDetail
{
    public class CreateDepreciationDetailCommandHandler : IRequestHandler<CreateDepreciationDetailCommand, string>
    {
        private readonly IMapper _mapper;
        private readonly IDepreciationDetailQueryRepository _depreciationDetailQueryRepository;
        private readonly IMediator _mediator;

        public CreateDepreciationDetailCommandHandler(
            IMapper mapper,
            IDepreciationDetailQueryRepository depreciationDetailQueryRepository,
            IMediator mediator)
        {
            _mapper = mapper;
            _depreciationDetailQueryRepository = depreciationDetailQueryRepository;
            _mediator = mediator;
        }

        public async Task<string> Handle(CreateDepreciationDetailCommand request, CancellationToken cancellationToken)
        {          
            // Call CreateAsync and get the status message and code
            var (creationMessage, statusCode) = await _depreciationDetailQueryRepository.CreateAsync(               
                request.finYearId,
                request.depreciationType,
                request.depreciationPeriod
            );  
            return statusCode switch
            {
                1 => creationMessage,
                -1 => throw new ExceptionRules(creationMessage),
                _ => throw new ExceptionRules("Unknown error occurred while creating depreciation detail.")
            };  
        }
    }
}
