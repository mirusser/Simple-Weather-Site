﻿using MongoDB.Driver;

namespace IconService.Services
{
    public interface IMongoCollectionFactory<TMongoDocument> where TMongoDocument : class
    {
        IMongoCollection<TMongoDocument> Create(string? collectionName = null);
    }
}