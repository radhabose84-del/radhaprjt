using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Events.Users
{
    public class AssetCreatedEvent
    {
        public Guid CorrelationId { get; set; }
        public int AssetId { get; set; }
        public int UserId { get; set; }
        public string? AssetName { get; set; }
    }
}