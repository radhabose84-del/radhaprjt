using Contracts.Common;
using UserManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace UserManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster
{
    public class DeleteMiscTypeMasterCommand : IRequest<ApiResponseDTO<GetMiscTypeMasterDto>>
    {
          public int Id { get; set; }
        
    }
}