using System.Threading;
using System.Threading.Tasks;

namespace BackgroundService.Application.Interfaces.Files
{
    public interface IFileFetcher
    {
        Task<byte[]> FetchAsync(string urlOrPath, CancellationToken ct);
    }
}
