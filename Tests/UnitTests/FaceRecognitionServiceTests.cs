using System.Threading.Tasks;
using Xunit;
using Moq;
using Core.Services;
using Core.Data;
using Core.Models;

public class FaceRecognitionServiceTests
{
    [Fact]
    public async Task RecognizeAsync_ReturnsNull_WhenNoFace()
    {
        var repo = new PeopleRepository();
        var service = new FaceRecognitionService(repo);
        var result = await service.RecognizeAsync(new byte[0]);
        Assert.Null(result);
    }

    [Fact]
    public async Task RecognizeAsync_ReturnsPerson_WhenMatchFound()
    {
        var mockRepo = new Mock<PeopleRepository>();
        var expected = new Person { Id = 1, Name = "Test" };
        mockRepo.Setup(r => r.GetAll()).Returns(new[] { expected });
        var service = new FaceRecognitionService(mockRepo.Object);
        // Assume ML returns expected on non-empty frame
        var result = await service.RecognizeAsync(new byte[] { 1, 2, 3 });
        Assert.Equal(expected, result);
    }
}