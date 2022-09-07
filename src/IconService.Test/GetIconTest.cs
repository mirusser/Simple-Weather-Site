using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using FluentAssertions;
using IconService.Messages.Queries;
using IconService.Models.Dto;
using IconService.Mongo.Documents;
using IconService.Mongo.Repository;
using IconService.Services;
using MediatR;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace IconService.Test
{
    public class GetIconTest
    {
        private GetIconQueryHandler? _sut { get; set; }
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IMongoRepository<IconDocument>> _iconMockMongoRepository = new();

        public GetIconTest()
        {
            Mock<IMongoCollection<IconDocument>> iconCollectionMock = new();

            var _iconMockCollectionFactory = new Mock<IMongoCollectionFactory<IconDocument>>();
            _iconMockCollectionFactory.Setup(x => x.Get(It.IsAny<string>())).Returns(iconCollectionMock.Object);
        }

        [Fact]
        public void ShouldImplementIRequest()
        {
            typeof(GetIconQueryHandler).Should().Implement<IRequestHandler<GetIconQuery, GetIconDto?>>();
        }

        [Fact]
        public async Task ShouldReturnIconIfExists()
        {
            //arrange
            var fixture = new Fixture();

            var iconDocument = fixture.Create<IconDocument>();

            //From what I've read Moq doesn't support setting up a specific expression, so that makes this test a little diluted.
            //https://stackoverflow.com/questions/2751935/moq-mockt-how-to-set-up-a-method-that-takes-an-expression
            _iconMockMongoRepository
                .Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<IconDocument, bool>>>(), It.IsAny<FindOptions?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(iconDocument);

            _mapperMock
                .Setup(x => x.Map<GetIconDto>(It.IsAny<IconDocument>()))
                .Returns(new GetIconDto()
                {
                    Id = iconDocument.Id,
                    DayIcon = iconDocument.DayIcon,
                    Description = iconDocument.Description,
                    FileContent = iconDocument.FileContent,
                    Icon = iconDocument.Icon,
                    Name = iconDocument.Name
                });

            var getIconQuery = new GetIconQuery() { Icon = iconDocument.Icon };
            _sut = new GetIconQueryHandler(_iconMockMongoRepository.Object, _mapperMock.Object);

            //act
            var iconDto = await _sut.Handle(getIconQuery, CancellationToken.None);

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

            _iconMockMongoRepository
                .Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<IconDocument, bool>>>(), It.IsAny<FindOptions?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => null);

            var getIconQuery = new GetIconQuery() { Icon = It.IsAny<string>() };
            _sut = new GetIconQueryHandler(_iconMockMongoRepository.Object, _mapperMock.Object);

            //act
            var iconDto = await _sut.Handle(getIconQuery, CancellationToken.None);

            //assert
            iconDto.Should().BeNull();
        }
    }
}