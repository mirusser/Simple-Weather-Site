using System.Threading;
using System.Threading.Tasks;

namespace CitiesService.Infrastructure.Database;

public interface IDatabaseBootstrapper
{
    Task EnsureDatabaseExistsAsync(CancellationToken cancellationToken);
}
