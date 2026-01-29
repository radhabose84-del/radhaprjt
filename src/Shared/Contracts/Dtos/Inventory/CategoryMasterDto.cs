using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Inventory
{
    public class CategoryMasterDto
    {
        public int Id { get; set; }
        public string ItemCategoryName { get; set; } = string.Empty;
    }
}