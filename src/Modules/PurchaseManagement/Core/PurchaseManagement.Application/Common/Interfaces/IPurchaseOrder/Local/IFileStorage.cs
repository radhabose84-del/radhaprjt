namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder;
public interface IFileStorage
{
    Task<string> PutAsync(string container, string key, byte[] data, string contentType, CancellationToken ct);
}
