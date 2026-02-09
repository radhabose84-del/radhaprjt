using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetManagement.Application.Common.HttpResponse;
using BudgetManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace BudgetManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster
{
    public class DeleteMiscTypeMasterCommand : IRequest<ApiResponseDTO<GetMiscTypeMasterDto>>
    {
          public int Id { get; set; }
    }
}