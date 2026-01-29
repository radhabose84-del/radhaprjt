using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Users
{
    public class AssetDto
    {
        public int AssetId { get; set; }
        public string? AssetName { get; set; }
        public int UserId { get; set; }
    }
}