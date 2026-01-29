using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using MediatR;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.AssetCategories.Command.CreateAssetCategories
{
    public class CreateAssetCategoriesCommand : IRequest<int>
    {
        //public string? Code { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public int AssetGroupId { get; set; }
        //public decimal GroupPercentage { get; set; }       
    }
}