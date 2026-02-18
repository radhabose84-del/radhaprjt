using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettings
{
    public class AdminSecuritySettingsStatusDto  :IRequest <ApiResponseDTO<AdminSecuritySettingsDto>>
    {
          public byte IsActive { get; set; }

    }
}