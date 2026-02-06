using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common;
using UserManagement.Application.Departments.Queries.GetDepartments;
using MediatR;
using UserManagement.Application.Common.Interfaces.IAdminSecuritySettings;
using AutoMapper;
using UserManagement.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettings;
using UserManagement.Application.Common.HttpResponse;

namespace UserManagement.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettingsById
{
    public class GetAdminSecuritySettingsByIdQuery  :IRequest<GetAdminSecuritySettingsDto>
    {
       
       
    public int Id { get; set; }


    }
}