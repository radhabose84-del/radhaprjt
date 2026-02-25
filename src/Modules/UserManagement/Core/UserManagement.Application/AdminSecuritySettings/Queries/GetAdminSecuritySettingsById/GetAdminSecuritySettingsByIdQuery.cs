using MediatR;
using UserManagement.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettings;

namespace UserManagement.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettingsById
{
    public class GetAdminSecuritySettingsByIdQuery  :IRequest<GetAdminSecuritySettingsDto>
    {
       
       
    public int Id { get; set; }


    }
}