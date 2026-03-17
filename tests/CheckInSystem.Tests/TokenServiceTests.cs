using CheckInSystem.Application.DTOs.Tokens;
using CheckInSystem.Application.Interfaces.Repositories;
using CheckInSystem.Application.Interfaces.Services;
using CheckInSystem.Domain.Entities;
using CheckInSystem.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CheckInSystem.Tests;

public sealed class TokenServiceTests
{
    [Fact]
    public async Task GenerateAsync_ShouldRevokeExistingToken_AndReturnNewCurrentToken()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "alice",
            FullName = "Alice",
            Email = "alice@example.com",
            IsActive = true
        };

        var previousToken = new UserToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            TokenId = "old-token-id",
            IssuedAtUtc = DateTime.UtcNow.AddHours(-2),
            ExpiresAtUtc = DateTime.UtcNow.AddHours(2),
            IsCurrent = true
        };

        UserToken? generatedToken = null;

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var userTokenRepository = new Mock<IUserTokenRepository>();
        userTokenRepository.SetupSequence(x => x.GetCurrentByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(previousToken)
            .ReturnsAsync(() => generatedToken);
        userTokenRepository.Setup(x => x.AddAsync(It.IsAny<UserToken>(), It.IsAny<CancellationToken>()))
            .Callback<UserToken, CancellationToken>((token, _) =>
            {
                token.User = user;
                generatedToken = token;
            })
            .Returns(Task.CompletedTask);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var jwtTokenService = new Mock<IJwtTokenService>();
        jwtTokenService.Setup(x => x.GenerateCheckInToken(user, It.IsAny<DateTime>()))
            .Returns(("jwt-token", "new-token-id", DateTime.UtcNow, DateTime.UtcNow.AddHours(24)));

        var service = new TokenService(
            userRepository.Object,
            userTokenRepository.Object,
            unitOfWork.Object,
            jwtTokenService.Object,
            NullLogger<TokenService>.Instance);

        var result = await service.GenerateAsync(user.Id, new GenerateUserTokenRequestDto { ValidHours = 24 }, "admin");

        Assert.Equal("jwt-token", result.Token);
        Assert.Equal("new-token-id", result.TokenInfo.TokenId);
        Assert.False(previousToken.IsCurrent);
        Assert.Equal("Regenerated", previousToken.RevokedReason);
        Assert.NotNull(previousToken.RevokedAtUtc);
        Assert.NotNull(generatedToken);
        Assert.True(generatedToken!.IsCurrent);
        Assert.Equal("admin", generatedToken.GeneratedBy);

        userTokenRepository.Verify(x => x.Update(previousToken), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
