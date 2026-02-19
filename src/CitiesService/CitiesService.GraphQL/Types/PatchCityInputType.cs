namespace CitiesService.GraphQL.Types;

public sealed class PatchCityInputType : InputObjectType<PatchCityInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<PatchCityInput> d)
    {
        d.Name("PatchCityInput");

        d.Field(x => x.Id).Type<NonNullType<IntType>>();

        // Explicitly make all patch fields nullable in the schema
        d.Field(x => x.CityId).Ignore();
        d.Field(x => x.Name).Type<StringType>();
        d.Field(x => x.State).Type<StringType>();
        d.Field(x => x.CountryCode).Type<StringType>();
        d.Field(x => x.Lon).Type<DecimalType>();
        d.Field(x => x.Lat).Type<DecimalType>();
        d.Field(x => x.RowVersion).Type<StringType>();
    }
}