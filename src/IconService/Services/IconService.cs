using Convey.Persistence.MongoDB;
using IconService.Mongo.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IconService.Services
{
    public class IconService : IIconService
    {
        private readonly IMongoRepository<IconDocument, string> _repository;

        public IconService(IMongoRepository<IconDocument, string> repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<IconDocument>> GetAll()
        {
            return await _repository.FindAsync(i => i.Id != null);
        }

        public async Task<IconDocument> GetAsync(string description, bool dayIcon = true)
        {
            return await _repository.GetAsync(i => i.Description == description && i.DayIcon == dayIcon);
        }

        public async Task<List<IconDocument>> CreateAsync(List<IconDocument> iconDocuments)
        {
            foreach (var iconDocument in iconDocuments)
            {
                await _repository.AddAsync(iconDocument);
            }

            return iconDocuments;
        }
    }
}
