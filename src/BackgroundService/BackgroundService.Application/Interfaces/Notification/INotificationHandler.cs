namespace BackgroundService.Application.Interfaces.Notification
{
    public interface INotificationHandler<TCommand>
    {
        Task ExecuteAsync(TCommand command, Guid correlationId);
    }
}