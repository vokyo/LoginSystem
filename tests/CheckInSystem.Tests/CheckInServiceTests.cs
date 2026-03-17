using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CheckInSystem.Application.Interfaces.Repositories;
using CheckInSystem.Application.Interfaces.Services;
using CheckInSystem.Domain.Entities;
using CheckInSystem.Domain.Enums;
using CheckInSystem.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CheckInSystem.Tests;

public sealed class CheckInServiceTests
{
    [Fact]
    public async Task CheckInAsync_ShouldReturnFailure_WhenTokenFormatIsInvalid()
    {
        CheckInRecord? savedRecord = null;

        var userRepository = new Mock<IUserRepository>();
        var userTokenRepository = new Mock<IUserTokenRepository>();
        userTokenRepository.Setup(x => x.GetByTokenIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserToken?)null);

        var recordRepository = new Mock<ICheckInRecordRepository>();
        recordRepository.Setup(x => x.AddAsync(It.IsAny<CheckInRecord>(), It.IsAny<CancellationToken>()))
            .Callback<CheckInRecord, CancellationToken>((record, _) => savedRecord = record)
            .Returns(Task.CompletedTask);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var jwtTokenService = new Mock<IJwtTokenService>();
        jwtTokenService.Setup(x => x.ValidateCheckInToken("invalid-token"))
            .Throws(new ArgumentException("Malformed token."));
        jwtTokenService.Setup(x => x.ReadIdentifiers("invalid-token"))
            .Throws(new ArgumentException("Malformed token."));

        var service = new CheckInService(
            userRepository.Object,
            userTokenRepository.Object,
            recordRepository.Object,
            unitOfWork.Object,
            jwtTokenService.Object,
            NullLogger<CheckInService>.Instance);

        var result = await service.CheckInAsync("invalid-token", "127.0.0.1", "xunit");

        Assert.False(result.IsSuccessful);
        Assert.Equal("Token format is invalid.", result.FailureReason);
        Assert.NotNull(savedRecord);
        Assert.Equal(CheckInRecordStatus.Failed, savedRecord!.Status);
        Assert.Equal("Token format is invalid.", savedRecord.FailureReason);
        Assert.Equal("127.0.0.1", savedRecord.SourceIp);
    }

    [Fact]
    public async Task CheckInAsync_ShouldCreateSuccessRecord_WhenTokenIsValidAndCurrent()
    {
        var userId = Guid.NewGuid();
        var tokenEntityId = Guid.NewGuid();
        var tokenId = "valid-token-id";

        var user = new User
        {
            Id = userId,
            UserName = "bob",
            FullName = "Bob",
            Email = "bob@example.com",
            IsActive = true
        };

        var currentToken = new UserToken
        {
            Id = tokenEntityId,
            UserId = userId,
            User = user,
            TokenId = tokenId,
            IssuedAtUtc = DateTime.UtcNow.AddHours(-1),
            ExpiresAtUtc = DateTime.UtcNow.AddHours(4),
            IsCurrent = true
        };

        CheckInRecord? savedRecord = null;

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var userTokenRepository = new Mock<IUserTokenRepository>();
        userTokenRepository.Setup(x => x.GetCurrentByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentToken);

        var recordRepository = new Mock<ICheckInRecordRepository>();
        recordRepository.Setup(x => x.AddAsync(It.IsAny<CheckInRecord>(), It.IsAny<CancellationToken>()))
            .Callback<CheckInRecord, CancellationToken>((record, _) => savedRecord = record)
            .Returns(Task.CompletedTask);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var jwtTokenService = new Mock<IJwtTokenService>();
        jwtTokenService.Setup(x => x.ValidateCheckInToken("valid-token"))
            .Returns(CreatePrincipal(userId, tokenId));

        var service = new CheckInService(
            userRepository.Object,
            userTokenRepository.Object,
            recordRepository.Object,
            unitOfWork.Object,
            jwtTokenService.Object,
            NullLogger<CheckInService>.Instance);

        var result = await service.CheckInAsync("valid-token", "127.0.0.1", "xunit");

        Assert.True(result.IsSuccessful);
        Assert.Equal("Success", result.Status);
        Assert.NotNull(savedRecord);
        Assert.Equal(CheckInRecordStatus.Success, savedRecord!.Status);
        Assert.Equal(userId, savedRecord.UserId);
        Assert.Equal(tokenEntityId, savedRecord.UserTokenId);
        Assert.Equal(tokenId, savedRecord.SubmittedTokenId);
    }

    private static ClaimsPrincipal CreatePrincipal(Guid userId, string tokenId)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, tokenId)
        ], "Test"));
    }
}
