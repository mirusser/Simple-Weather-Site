using IconService.Mongo.Documents;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IconService.Services
{
    public interface IIconService
    {
        Task<IReadOnlyList<IconDocument>> GetAll();
        Task<IconDocument> GetAsync(string description, bool dayIcon = true);
        Task<List<IconDocument>> CreateAsync(List<IconDocument> iconDocuments);
    }
}