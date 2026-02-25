#nullable disable
using AutoMapper;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.ICompany;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;

namespace UserManagement.Application.Companies.Commands.UpdateCompany
{
    public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, bool>
    {
        private readonly ICompanyCommandRepository _icompanyRepository;
        private readonly IFileUploadService _ifileUploadService;
        private readonly IMapper _imapper;
        private readonly ICompanyQueryRepository _companyQueryRepository;
        private readonly IMediator _mediator;

        public UpdateCompanyCommandHandler(ICompanyCommandRepository icompanyRepository, IMapper imapper, IFileUploadService ifileUploadService, ICompanyQueryRepository companyQueryRepository, IMediator mediator)
        {
            _icompanyRepository = icompanyRepository;
            _imapper = imapper;
            _ifileUploadService = ifileUploadService;
            _companyQueryRepository = companyQueryRepository;
            _mediator = mediator;
        }

          public async Task<bool> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
        {
            
            
            var existingCompany = await _companyQueryRepository.GetByCompanynameAsync(request.Company.CompanyName,request.Company.Id);

              if (existingCompany != null)
              {
                throw new ValidationException("Company already exists");
                  
              }
            var company  = _imapper.Map<Company>(request.Company);
            
            var  CompanyId = await _icompanyRepository.UpdateAsync(request.Company.Id, company);
           
           if (CompanyId)
           {
             
                 var domainEvent = new AuditLogsDomainEvent(
                     actionDetail: "Update",
                     actionCode: "",
                     actionName: "",
                     details: $"Company '{company.Id}' was updated.",
                     module:"Company"
                 );
                 await _mediator.Publish(domainEvent, cancellationToken);
               return CompanyId;
           }
           throw new Exception("Company not updated");
            
        }
    }
}