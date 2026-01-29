using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettings
{
    public class AdminSecuritySettingsStatusDto  :IRequest <ApiResponseDTO<AdminSecuritySettingsDto>>
    {
          public byte IsActive { get; set; }

    }
}