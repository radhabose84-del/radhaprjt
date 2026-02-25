using MediatR;


namespace UserManagement.Application.AdminSecuritySettings.Commands.DeleteAdminSecuritySettings
{
    public class DeleteAdminSecuritySettingsCommand : IRequest<int>
    {

         public int Id { get; set; }      
    }
}