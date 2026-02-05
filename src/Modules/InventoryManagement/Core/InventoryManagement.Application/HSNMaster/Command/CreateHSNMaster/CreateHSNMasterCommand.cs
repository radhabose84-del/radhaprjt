using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Application.Common.HttpResponse;
using MediatR;

namespace InventoryManagement.Application.HSNMaster.Command.CreateHSNMaster
{
    public class CreateHSNMasterCommand  : IRequest<ApiResponseDTO<int>>
    {
        public int TypeId { get; set; }
        public string HSNCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int GSTCategoryId { get; set; }
        public decimal GSTPercentage { get; set; }
        public decimal IGSTPercentage { get; set; }
        public DateTimeOffset ValidFrom { get; set; }
        
    }
}