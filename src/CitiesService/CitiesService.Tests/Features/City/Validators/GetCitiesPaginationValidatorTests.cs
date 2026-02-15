using CitiesService.Application.Features.City.Queries.GetCitiesPagination;
using FluentValidation.TestHelper;

namespace CitiesService.Tests.Features.City.Validators;

public class GetCitiesPaginationValidatorTests
{
    private readonly GetCitiesPaginationValidator sut = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NumberOfCities_LessThanOne_IsInvalid(int numberOfCities)
    {
        var model = new GetCitiesPaginationQuery { NumberOfCities = numberOfCities, PageNumber = 1 };
        var result = sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.NumberOfCities);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void NumberOfCities_InRange_IsValid(int numberOfCities)
    {
        var model = new GetCitiesPaginationQuery { NumberOfCities = numberOfCities, PageNumber = 1 };
        var result = sut.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.NumberOfCities);
    }

    [Fact]
    public void NumberOfCities_Over100_IsInvalid()
    {
        var model = new GetCitiesPaginationQuery { NumberOfCities = 101, PageNumber = 1 };
        var result = sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.NumberOfCities);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PageNumber_LessThanOne_IsInvalid(int pageNumber)
    {
        var model = new GetCitiesPaginationQuery { NumberOfCities = 10, PageNumber = pageNumber };
        var result = sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
    }

    [Fact]
    public void PageNumber_One_IsValid()
    {
        var model = new GetCitiesPaginationQuery { NumberOfCities = 10, PageNumber = 1 };
        var result = sut.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.PageNumber);
    }
}
