using UserManagement.Domain.Entities;
using MediatR;
using AutoMapper;
using UserManagement.Application.Common.Interfaces.ICompany;
using UserManagement.Domain.Events;
using FluentValidation;

namespace UserManagement.Application.Companies.Commands.DeleteCompany
{
    public class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand, bool>
    {
        private readonly ICompanyCommandRepository _icompanyRepository;
        private readonly ICompanyQueryRepository _companyQueryRepository;
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;

        public DeleteCompanyCommandHandler(ICompanyCommandRepository companyRepository,ICompanyQueryRepository companyQueryRepository,IMapper imapper,IMediator mediator)
        {
            _icompanyRepository = companyRepository;
            _companyQueryRepository = companyQueryRepository;
            _imapper = imapper;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
        {
            var existingCompany = await _companyQueryRepository.GetByIdAsync(request.Id);
            if (existingCompany == null)
                throw new ValidationException("Invalid CompanyId. Company not found / already deleted.");

            var usedByUsers = await _companyQueryRepository.IsCompanyUsedByAnyUserAsync(request.Id);
            if (usedByUsers)
                throw new ValidationException("Cannot delete Company : this record is referenced by other data.");

            var company = _imapper.Map<Company>(request);

            var result = await _icompanyRepository.DeleteAsync(request.Id, company);

            if (result)
            {
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Delete",
                    actionCode: "",
                    actionName: "",
                    details: $"Company '{request.Id}' was deleted.",
                    module: "Company"
                );

                await _mediator.Publish(domainEvent, cancellationToken);
                return true;
            }

            throw new Exception("Company not deleted");
        }
    }
}
