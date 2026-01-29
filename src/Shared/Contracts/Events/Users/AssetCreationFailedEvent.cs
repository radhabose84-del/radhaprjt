using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Events.Users
{
    public class AssetCreationFailedEvent
    {
        public Guid CorrelationId { get; set; }
        public string Reason { get; set; }
    }
}