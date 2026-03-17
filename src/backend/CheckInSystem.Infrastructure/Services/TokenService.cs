using CheckInSystem.Application.Common.Exceptions;
using CheckInSystem.Application.DTOs.Tokens;
using CheckInSystem.Application.Interfaces.Repositories;
using CheckInSystem.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CheckInSystem.Infrastructure.Services;

public sealed class TokenService(
    IUserRepository userRepository,
    IUserTokenRepository userTokenRepository,
    IUnitOfWork unitOfWork,
    IJwtTokenService jwtTokenService,
    ILogger<TokenService> logger) : ITokenService
{
    public async Task<TokenGenerationResultDto> GenerateAsync(Guid userId, GenerateUserTokenRequestDto request, string generatedBy, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken) ?? throw new KeyNotFoundException("User not found.");
        if (!user.IsActive)
        {
            throw new BusinessException("Cannot generate a token for a disabled user.");
        }

        var currentToken = await userTokenRepository.GetCurrentByUserIdAsync(userId, cancellationToken);
        if (currentToken is not null)
        {
            currentToken.IsCurrent = false;
            currentToken.RevokedAtUtc = DateTime.UtcNow;
            currentToken.RevokedReason = "Regenerated";
            userTokenRepository.Update(currentToken);
        }

        var expiresAtUtc = DateTime.UtcNow.AddHours(request.ValidHours);
        var (token, tokenId, issuedAtUtc, generatedExpiresAtUtc) = jwtTokenService.GenerateCheckInToken(user, expiresAtUtc);

        var tokenEntity = new Domain.Entities.UserToken
        {
            UserId = user.Id,
            TokenId = tokenId,
            IssuedAtUtc = issuedAtUtc,
            ExpiresAtUtc = generatedExpiresAtUtc,
            IsCurrent = true,
            GeneratedBy = generatedBy
        };

        await userTokenRepository.AddAsync(tokenEntity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Generated a new check-in token for user {UserName}.", user.UserName);
        var current = await userTokenRepository.GetCurrentByUserIdAsync(userId, cancellationToken) ?? throw new KeyNotFoundException("Generated token was not found.");

        return new TokenGenerationResultDto
        {
            Token = token,
            TokenInfo = current.ToDto()
        };
    }

    public async Task<UserTokenDto?> GetCurrentAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var token = await userTokenRepository.GetCurrentByUserIdAsync(userId, cancellationToken);
        return token?.ToDto();
    }

    public async Task<IReadOnlyCollection<UserTokenDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await userTokenRepository.GetByUserIdAsync(userId, cancellationToken);
        return tokens.Select(x => x.ToDto()).ToArray();
    }

    public async Task<UserTokenDto> RevokeCurrentAsync(Guid userId, string revokedBy, CancellationToken cancellationToken = default)
    {
        _ = await userRepository.GetByIdAsync(userId, cancellationToken) ?? throw new KeyNotFoundException("User not found.");

        var token = await userTokenRepository.GetCurrentByUserIdAsync(userId, cancellationToken)
                    ?? throw new KeyNotFoundException("Current token not found.");

        if (token.RevokedAtUtc.HasValue || !token.IsCurrent)
        {
            throw new BusinessException("Current token is already inactive.");
        }

        token.IsCurrent = false;
        token.RevokedAtUtc = DateTime.UtcNow;
        token.RevokedReason = $"Revoked by {revokedBy}";
        userTokenRepository.Update(token);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Revoked current token {TokenId} for user {UserId}.", token.TokenId, userId);
        return token.ToDto();
    }
}
