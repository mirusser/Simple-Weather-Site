using CitiesService.Domain.Entities;

namespace CitiesService.GraphQL;

public class CityInfoType : ObjectType<CityInfo>
{
    protected override void Configure(IObjectTypeDescriptor<CityInfo> d)
    {
        d.Name("City");

        d.Field(x => x.Id).Type<NonNullType<IntType>>();
        d.Field(x => x.CityId).Name("cityId").Type<NonNullType<DecimalType>>();
        d.Field(x => x.Name).Type<NonNullType<StringType>>();
        d.Field(x => x.State).Type<StringType>();
        d.Field(x => x.CountryCode).Name("countryCode").Type<NonNullType<StringType>>();
        d.Field(x => x.Lat).Type<NonNullType<DecimalType>>();
        d.Field(x => x.Lon).Type<NonNullType<DecimalType>>();
    }
}