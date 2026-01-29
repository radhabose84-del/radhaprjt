using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Application.CompanySettings.Queries.GetCompanySettings;
using MediatR;

namespace Core.Application.CompanySettings.Queries.GetCompanySettingsById
{
    public class GetCompanySettingByIdQuery : IRequest<ApiResponseDTO<CompanySettingsDTO>>
    {
        // public int Id { get; set; }
    }
}