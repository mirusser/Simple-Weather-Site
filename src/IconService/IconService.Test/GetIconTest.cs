using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
//using AutoMapper;
using ErrorOr;
using FluentAssertions;
using IconService.Application.Common.Interfaces.Persistence;
using IconService.Application.Icon.Get;
using IconService.Application.Icon.Models.Dto;
using IconService.Domain.Entities.Documents;
using MapsterMapper;
using MediatR;
using MongoDB.Driver;
using Moq;
using Xunit;
using static IconService.Domain.Common.Errors.Errors;

namespace IconService.Test
{
    public class GetIconTest
    {
        private GetQueryHandler? _sut { get; set; }
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
            typeof(GetQueryHandler).Should().Implement<IRequestHandler<GetQuery, ErrorOr<GetResult>>>();
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
            _sut = new GetQueryHandler(_iconMockMongoRepository.Object, _mapperMock.Object);

            //act
            var iconDto = await _sut.Handle(getIconQuery, CancellationToken.None);

            //assert
            iconDto
                .Should()
                .NotBeNull()
                .And
                .Equals(getIconQuery.Icon);
            iconDto.Value.Id.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ShouldReturnNothingIfIconDoesNotExists()
        {
            //arrange
            var fixture = new Fixture();

            _iconMockMongoRepository
                .Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<IconDocument, bool>>>(), It.IsAny<FindOptions?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => null);

            var getIconQuery = new GetQuery(It.IsAny<string>());
            _sut = new GetQueryHandler(_iconMockMongoRepository.Object, _mapperMock.Object);

            //act
            var iconDto = await _sut.Handle(getIconQuery, CancellationToken.None);

            //assert
            iconDto.Should().BeNull();
        }
    }
}