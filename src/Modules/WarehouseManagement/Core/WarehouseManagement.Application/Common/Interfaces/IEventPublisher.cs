using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WarehouseManagement.Application.Common.Interfaces
{
    public interface IEventPublisher
    {
        Task SaveEventAsync<T>(T @event) where T : class;
        Task PublishPendingEventsAsync();
    }
}