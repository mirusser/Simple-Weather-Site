using IconService.Application.Icon.Commands.Create;
using IconService.Application.Icon.Models.Dto;
using IconService.Application.Icon.Queries.Get;
using IconService.Contracts.Icon;
using IconService.Domain.Entities.Documents;
using Mapster;

namespace IconService.Application.Common.Mapping;

public class IconMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<GetResult, IconDocument>();
        config.NewConfig<GetRequest, GetQuery>();
        config.NewConfig<GetResponse, GetResult>();

        config.NewConfig<IconDocument, CreateResult>();
        config.NewConfig<CreateRequest, CreateCommand>();
        config.NewConfig<CreateResponse, CreateResult>();

        config.NewConfig<IEnumerable<IconDocument>, IEnumerable<CreateResult>>();

        config.NewConfig<CreateCommand, IconDocument>();
    }
}