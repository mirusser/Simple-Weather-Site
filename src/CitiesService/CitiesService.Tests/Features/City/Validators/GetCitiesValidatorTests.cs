using CitiesService.Application.Features.City.Queries.GetCities;
using FluentValidation.TestHelper;

namespace CitiesService.Tests.Features.City.Validators;

public class GetCitiesValidatorTests
{
    private readonly GetCitiesValidator sut = new();

    [Fact]
    public void CityName_Null_IsInvalid()
    {
#pragma warning disable CS8625
        var model = new GetCitiesQuery { CityName = null, Limit = 10 };
#pragma warning restore CS8625

        var result = sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CityName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData(" a ")]
    public void CityName_EmptyOrWhitespace_IsInvalid(string input)
    {
        var model = new GetCitiesQuery { CityName = input, Limit = 10 };

        var result = sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CityName);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("  ab  ")]
    [InlineData("New York")]
    public void CityName_AtLeastTwoNonWhitespaceChars_IsValid(string input)
    {
        var model = new GetCitiesQuery { CityName = input, Limit = 10 };

        var result = sut.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.CityName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Limit_LessThanOne_IsInvalid(int limit)
    {
        var model = new GetCitiesQuery { CityName = "ab", Limit = limit };

        var result = sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Limit);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void Limit_InRange_IsValid(int limit)
    {
        var model = new GetCitiesQuery { CityName = "ab", Limit = limit };

        var result = sut.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Limit);
    }

    [Fact]
    public void Limit_Over100_IsInvalid()
    {
        var model = new GetCitiesQuery { CityName = "ab", Limit = 101 };

        var result = sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Limit);
    }
}
