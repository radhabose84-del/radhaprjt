using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common;
using UserManagement.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettings;
using MediatR;
using UserManagement.Application.Common.HttpResponse;


namespace UserManagement.Application.AdminSecuritySettings.Commands.DeleteAdminSecuritySettings
{
    public class DeleteAdminSecuritySettingsCommand : IRequest<int>
    {

         public int Id { get; set; }      
    }
}