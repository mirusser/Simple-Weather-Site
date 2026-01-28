using System.Threading;
using System.Threading.Tasks;

namespace CitiesService.Application.Features.City.Services;

public interface ICitiesSeeder
{
    Task<bool> SeedIfEmptyAsync(CancellationToken cancellationToken);
}
