using Contracts.Common;
using UserManagement.Application.CompanySettings.Queries.GetCompanySettings;
using MediatR;

namespace UserManagement.Application.CompanySettings.Queries.GetCompanySettingsById
{
    public class GetCompanySettingByIdQuery : IRequest<ApiResponseDTO<CompanySettingsDTO>>
    {
        // public int Id { get; set; }
    }
}