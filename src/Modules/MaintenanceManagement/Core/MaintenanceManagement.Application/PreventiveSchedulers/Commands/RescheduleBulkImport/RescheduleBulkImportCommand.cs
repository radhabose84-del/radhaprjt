using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.RescheduleBulkImport
{
    public class RescheduleBulkImportCommand : IRequest<ApiResponseDTO<string>>
    {
        public IFormFile File { get; set; }
    }
}