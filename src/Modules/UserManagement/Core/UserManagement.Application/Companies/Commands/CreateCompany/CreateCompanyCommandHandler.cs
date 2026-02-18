using UserManagement.Domain.Entities;
using UserManagement.Application.Common.Interfaces;
using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.ICompany;
using Contracts.Common;
using UserManagement.Domain.Events;

namespace UserManagement.Application.Companies.Commands.CreateCompany
{
    public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, int>
    {
         private readonly ICompanyCommandRepository _icompanyRepository;
         private readonly IFileUploadService _ifileUploadService;
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;
        private readonly ICompanyQueryRepository _companyQueryRepository;

        public CreateCompanyCommandHandler(ICompanyCommandRepository icompanyRepository, IMapper imapper, IFileUploadService ifileUploadService, IMediator mediator, ICompanyQueryRepository companyQueryRepository)
        {
            _icompanyRepository = icompanyRepository;
            _imapper = imapper;
            _ifileUploadService = ifileUploadService;
            _mediator = mediator;
            _companyQueryRepository = companyQueryRepository;
        }

        public async Task<int> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
        {
            // var existingCompany = await _companyQueryRepository.GetByCompanynameAsync(request.Company.CompanyName);

            //   if (existingCompany != null)
            //   {
            //       return new ApiResponseDTO<int>{IsSuccess = false, Message = "Company already exists"};
            //   }
             var company  = _imapper.Map<Company>(request.Company);
           
            
             var CompanyId = await _icompanyRepository.CreateAsync(company);

              //Domain Event
                 var domainEvent = new AuditLogsDomainEvent(
                     actionDetail: "Create",
                     actionCode: "",
                     actionName: "",
                     details: $"Company '{CompanyId}' was created.",
                     module:"Company"
                 );
                 await _mediator.Publish(domainEvent, cancellationToken);

                if (CompanyId > 0)
                {
                    return CompanyId;
                }
                throw new Exception("Company not created");
                
        }
    }
}