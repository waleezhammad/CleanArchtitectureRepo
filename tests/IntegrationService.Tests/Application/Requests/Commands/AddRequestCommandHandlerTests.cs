using IntegrationService.Application.Common.Interfaces;
using IntegrationService.Application.Requests.Commands.AddRequest;
using IntegrationService.Domain.Common;
using IntegrationService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IntegrationService.Tests.Application.Requests.Commands;

public class AddRequestCommandHandlerTests
{
    private readonly Mock<IIntegrationClient> _integrationClientMock;
    private readonly Mock<IRequestRepository> _requestRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<AddRequestCommandHandler>> _loggerMock;
    private readonly AddRequestCommandHandler _handler;

    public AddRequestCommandHandlerTests()
    {
        _integrationClientMock = new Mock<IIntegrationClient>();
        _requestRepositoryMock = new Mock<IRequestRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<AddRequestCommandHandler>>();

        _handler = new AddRequestCommandHandler(
            _integrationClientMock.Object,
            _requestRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var command = new AddRequestCommand
        {
            RequestType = "TestType",
            RequestData = "Test data",
            Metadata = new Dictionary<string, string>()
        };

        var externalResponse = new AddRequestResult
        {
            ExternalRequestId = "EXT-12345",
            Status = "Submitted",
            SubmittedAt = DateTime.UtcNow
        };

        _integrationClientMock
            .Setup(x => x.AddRequestAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(externalResponse));

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(externalResponse.ExternalRequestId, result.Value.ExternalRequestId);

        _requestRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ExternalAPIFailure_ReturnsFailureResult()
    {
        // Arrange
        var command = new AddRequestCommand
        {
            RequestType = "TestType",
            RequestData = "Test data",
            Metadata = new Dictionary<string, string>()
        };

        _integrationClientMock
            .Setup(x => x.AddRequestAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AddRequestResult>("External API error"));

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("External API error", result.Error);

        _requestRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
