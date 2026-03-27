#nullable disable
namespace BSOFT.Worker.Services;

public interface IWorkerNotificationService
{
    Task SendAsync(string method, object payload);
}
