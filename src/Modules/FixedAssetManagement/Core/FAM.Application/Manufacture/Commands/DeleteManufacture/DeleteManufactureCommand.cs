using FAM.Application.Manufacture.Queries.GetManufacture;
using MediatR;
using Contracts.Common;

namespace FAM.Application.Manufacture.Commands.DeleteManufacture
{
    public class DeleteManufactureCommand :  IRequest<ManufactureDTO>, IRequirePermission
    {
        public int Id { get; set; }  
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
