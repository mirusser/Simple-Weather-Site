using Convey.CQRS.Queries;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WeatherHistoryService.Mongo.Documents;

namespace WeatherHistoryService.Mongo.Contracts.Repositories
{
    public interface ICityWeatherForecastRepository
    {
        Task<CityWeatherForecastDocument> AddAsync(CityWeatherForecastDocument entity);
        Task<PagedResult<CityWeatherForecastDocument>> BrowseAsync<TQuery>(Expression<Func<CityWeatherForecastDocument, bool>> predicate, TQuery query) where TQuery : IPagedQuery;
        Task DeleteAsync(Expression<Func<CityWeatherForecastDocument, bool>> predicate);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Expression<Func<CityWeatherForecastDocument, bool>> predicate);
        Task<IReadOnlyList<CityWeatherForecastDocument>> FindAsync(Expression<Func<CityWeatherForecastDocument, bool>> predicate);
        Task<CityWeatherForecastDocument> GetAsync(Expression<Func<CityWeatherForecastDocument, bool>> predicate);
        Task<CityWeatherForecastDocument> GetAsync(string id);
        Task UpdateAsync(CityWeatherForecastDocument entity);
        Task UpdateAsync(CityWeatherForecastDocument entity, Expression<Func<CityWeatherForecastDocument, bool>> predicate);
    }
}