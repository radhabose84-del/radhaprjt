using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Common.Interfaces.ICompanySettings;
using MediatR;

namespace UserManagement.Application.CompanySettings.Commands.UpdateCompanySettings
{
    public class UpdateCompanySettingsCommandHandler : IRequestHandler<UpdateCompanySettingsCommand, bool>
    {
        private readonly ICompanyCommandSettings _icompanyCommandSettings;
        private readonly IMediator _mediator;
        private readonly IMapper _imapper;

        public UpdateCompanySettingsCommandHandler(ICompanyCommandSettings icompanyCommandSettings, IMediator mediator, IMapper imapper)
        {
            _icompanyCommandSettings = icompanyCommandSettings;
            _mediator = mediator;
            _imapper = imapper;
        }
        public async Task<bool> Handle(UpdateCompanySettingsCommand request, CancellationToken cancellationToken)
        {
            var companySettings = _imapper.Map<UserManagement.Domain.Entities.CompanySettings>(request);

            var CompanySettingResponse = await _icompanyCommandSettings.UpdateAsync(request.Id, companySettings);

            if(CompanySettingResponse)
            {
                return CompanySettingResponse;
            }
            throw new Exception("Company Settings not updated");
            
        }
    }
}