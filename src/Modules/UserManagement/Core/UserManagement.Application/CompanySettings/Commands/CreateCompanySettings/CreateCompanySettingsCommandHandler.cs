using AutoMapper;
using UserManagement.Application.Common.Interfaces.ICompanySettings;
using UserManagement.Domain.Events;
using MediatR;

namespace UserManagement.Application.CompanySettings.Commands.CreateCompanySettings
{
    public class CreateCompanySettingsCommandHandler : IRequestHandler<CreateCompanySettingsCommand, int>
    {
        private readonly ICompanyCommandSettings _icompanyCommandSettings;
        private readonly IMediator _mediator;
        private readonly IMapper _imapper;
        public CreateCompanySettingsCommandHandler(ICompanyCommandSettings icompanyCommandSettings, IMediator mediator, IMapper imapper)
        {
            _icompanyCommandSettings = icompanyCommandSettings;   
            _mediator = mediator;
            _imapper = imapper;
        }
        public async Task<int> Handle(CreateCompanySettingsCommand request, CancellationToken cancellationToken)
        {
            var companySettings = _imapper.Map<UserManagement.Domain.Entities.CompanySettings>(request);
            
            
            var CompanySettingsId =  await _icompanyCommandSettings.CreateAsync(companySettings);
            if (CompanySettingsId > 0)
            {
                     var domainEvent = new AuditLogsDomainEvent(
                     actionDetail: "Create",
                     actionCode: "",
                     actionName: "",
                     details: $"Company Setting '{CompanySettingsId}' was created.",
                     module:"Company Setting"
                 );
                 await _mediator.Publish(domainEvent, cancellationToken);
                 
                return CompanySettingsId;
            }
            throw new Exception("Company Settings not created");
            
        }
    }
}