using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common;
using Core.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettings;
using MediatR;
using Core.Application.Common.HttpResponse;


namespace Core.Application.AdminSecuritySettings.Commands.DeleteAdminSecuritySettings
{
    public class DeleteAdminSecuritySettingsCommand : IRequest<int>
    {

         public int Id { get; set; }      
    }
}