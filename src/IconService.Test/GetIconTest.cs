using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using IconService.Messages.Queries;
using IconService.Models.Dto;
using IconService.Mongo.Documents;
using IconService.Mongo.Repository;
using IconService.Services;
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
            _iconMockCollectionFactory.Setup(x => x.Create(It.IsAny<string>())).Returns(iconCollectionMock.Object);
        }

        [Fact]
        public async Task ShouldReturnIconIfExists()
        {
            //arrange
            var fixture = new Fixture();

            IconDocument iconDocument = fixture.Create<IconDocument>();
            var getIconQuery = new GetIconQuery() { Icon = iconDocument.Icon };

            Mock<IMongoCollection<IconDocument>> iconCollectionMock = new();

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

            _sut = new GetIconQueryHandler(_iconMockMongoRepository.Object, _mapperMock.Object);

            //act
            var iconDto = await _sut.Handle(getIconQuery, CancellationToken.None);

            //assert
            Assert.NotNull(iconDto);
            Assert.Equal(getIconQuery.Icon, iconDto.Icon);
            Assert.NotEmpty(iconDto.Id);
        }
    }
}