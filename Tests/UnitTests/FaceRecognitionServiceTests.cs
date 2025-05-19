using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Core.Data;
using Core.Models;
using Core.Services;

namespace Tests.UnitTests
{
    public class FaceRecognitionServiceTests
    {
        private readonly Mock<PeopleRepository> _repoMock;
        private readonly FaceRecognitionService _service;

        public FaceRecognitionServiceTests()
        {
            _repoMock = new Mock<PeopleRepository>();
            _service = new FaceRecognitionService(_repoMock.Object);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(new byte[0])]
        public async Task RecognizeAsync_InvalidFrame_ReturnsNull(byte[] frame)
        {
            var result = await _service.RecognizeAsync(frame);
            Assert.Null(result);
            _repoMock.Verify(r => r.GetAll(), Times.Never);
        }

        [Fact]
        public async Task RecognizeAsync_NoPersonsInRepo_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetAll()).Returns(Array.Empty<Person>());
            var result = await _service.RecognizeAsync(new byte[] { 0x01 });
            Assert.Null(result);
            _repoMock.Verify(r => r.GetAll(), Times.Once);
        }

        [Fact]
        public async Task RecognizeAsync_FirstMatchInRepo_ReturnsThatPerson()
        {
            var candidate = new Person("Alice Smith", new byte[] { 0xAA, 0xBB, 0xCC });
            _repoMock.Setup(r => r.GetAll()).Returns(new[] { candidate, new Person("Bob Jones", new byte[]{1,2,3}) });
            var result = await _service.RecognizeAsync(new byte[] { 0x01, 0x02 });
            Assert.NotNull(result);
            Assert.Equal("Alice Smith", result.FullName);
        }

        [Fact]
        public async Task RecognizeAsync_CallsRepositoryExactlyOnce()
        {
            _repoMock.Setup(r => r.GetAll()).Returns(Enumerable.Empty<Person>());
            await _service.RecognizeAsync(new byte[] { 0xFF });
            _repoMock.Verify(r => r.GetAll(), Times.Once);
        }

        [Fact]
        public async Task RecognizeAsync_MultipleCalls_AreIndependent()
        {
            var first = new Person("First", new byte[]{1});
            var second = new Person("Second", new byte[]{2});
            _repoMock.SetupSequence(r => r.GetAll())
                     .Returns(new[] { first })
                     .Returns(new[] { second });

            var r1 = await _service.RecognizeAsync(new byte[]{1});
            var r2 = await _service.RecognizeAsync(new byte[]{2});

            Assert.Equal("First", r1.FullName);
            Assert.Equal("Second", r2.FullName);
        }
    }
}
