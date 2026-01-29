using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.FixedAsset
{
    public class AssetGroupDto
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? GroupName { get; set; }
    }
}