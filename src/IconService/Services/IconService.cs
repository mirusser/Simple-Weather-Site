using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IconService.Mongo.Documents;
using MongoDB.Driver;

namespace IconService.Services
{
    public class IconService : IIconService
    {
        private readonly IMongoCollection<IconDocument> _iconCollection;

        public IconService(IMongoCollection<IconDocument> iconCollection)
        {
            _iconCollection = iconCollection;
        }

        public async Task<IReadOnlyList<IconDocument>> GetAll()
        {
            return (await _iconCollection.FindAsync(i => i.Id != null)).ToList().AsReadOnly();
        }

        public async Task<IconDocument> GetAsync(string icon)
        {
            return await (await _iconCollection.FindAsync(i => i.Id != null)).FirstOrDefaultAsync();
        }


    }
}