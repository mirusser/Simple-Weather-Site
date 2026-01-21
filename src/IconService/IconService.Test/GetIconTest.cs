using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Common.Mediator;
using FluentAssertions;
using IconService.Application.Common.Interfaces.Persistence;
using IconService.Application.Icon.Models.Dto;
using IconService.Application.Icon.Queries.Get;
using IconService.Domain.Entities.Documents;
using MapsterMapper;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace IconService.Test
{
    public class GetIconTest
    {
        private GetQueryHandler? sut { get; set; }
        private readonly Mock<IMapper> mapperMock = new();
        private readonly Mock<IMongoRepository<IconDocument>> iconMockMongoRepository = new();

        public GetIconTest()
        {
            Mock<IMongoCollection<IconDocument>> iconCollectionMock = new();

            var iconMockCollectionFactory = new Mock<IMongoCollectionFactory<IconDocument>>();
            iconMockCollectionFactory
                .Setup(x => x.Get(It.IsAny<string>()))
                .Returns(iconCollectionMock.Object);
        }

        [Fact]
        public void ShouldImplementIRequest()
        {
            typeof(GetQueryHandler).Should().Implement<IRequestHandler<GetQuery, GetResult>>();
        }

        [Fact]
        public async Task ShouldReturnIconIfExists()
        {
            //arrange
            var fixture = new Fixture();

            var iconDocument = fixture.Create<IconDocument>();

            //From what I've read Moq doesn't support setting up a specific expression, so that makes this test a little diluted.
            //https://stackoverflow.com/questions/2751935/moq-mockt-how-to-set-up-a-method-that-takes-an-expression
            iconMockMongoRepository
                .Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<IconDocument, bool>>>(), It.IsAny<FindOptions?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(iconDocument);

            mapperMock
                .Setup(x => x.Map<GetResult>(It.IsAny<IconDocument>()))
                .Returns(new GetResult()
                {
                    Id = iconDocument.Id,
                    DayIcon = iconDocument.DayIcon,
                    Description = iconDocument.Description,
                    FileContent = iconDocument.FileContent,
                    Icon = iconDocument.Icon,
                    Name = iconDocument.Name
                });

            var getIconQuery = new GetQuery(iconDocument.Icon);
            sut = new GetQueryHandler(iconMockMongoRepository.Object, mapperMock.Object);

            //act
            var iconDto = await sut.Handle(getIconQuery, CancellationToken.None);

            //assert
            iconDto
                .Should()
                .NotBeNull()
                .And
                .Equals(getIconQuery.Icon);
            iconDto.Id.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ShouldReturnNothingIfIconDoesNotExists()
        {
            //arrange
            var fixture = new Fixture();

            iconMockMongoRepository
                .Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<IconDocument, bool>>>(), It.IsAny<FindOptions?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => null);

            var getIconQuery = new GetQuery(It.IsAny<string>());
            sut = new GetQueryHandler(iconMockMongoRepository.Object, mapperMock.Object);

            //act
            var iconDto = await sut.Handle(getIconQuery, CancellationToken.None);

            //assert
            iconDto.Should().BeNull();
        }
    }
}