namespace MaintenanceManagement.Application.Common.Interfaces
{
    public interface IEventPublisher
    {
        Task SaveEventAsync<T>(T @event) where T : class;
        Task PublishPendingEventsAsync();
    }
}