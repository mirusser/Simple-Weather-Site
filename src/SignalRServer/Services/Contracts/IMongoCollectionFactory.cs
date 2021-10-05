using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace SignalRServer.Services.Contracts
{
    public interface IMongoCollectionFactory<TMongoDocument> where TMongoDocument : class
    {
        IMongoCollection<TMongoDocument> Create(string? collectionName = null);
    }
}