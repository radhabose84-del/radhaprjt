using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.RescheduleBulkImport
{
    public class RescheduleBulkImportCommand : IRequest<ApiResponseDTO<string>>
    {
        public IFormFile File { get; set; } = default!;
    }
}