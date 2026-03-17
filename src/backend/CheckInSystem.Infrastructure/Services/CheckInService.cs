using System.IdentityModel.Tokens.Jwt;
using CheckInSystem.Application.Common.Models;
using CheckInSystem.Application.DTOs.CheckIns;
using CheckInSystem.Application.Interfaces.Repositories;
using CheckInSystem.Application.Interfaces.Services;
using CheckInSystem.Domain.Entities;
using CheckInSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace CheckInSystem.Infrastructure.Services;

public sealed class CheckInService(
    IUserRepository userRepository,
    IUserTokenRepository userTokenRepository,
    ICheckInRecordRepository checkInRecordRepository,
    IUnitOfWork unitOfWork,
    IJwtTokenService jwtTokenService,
    ILogger<CheckInService> logger) : ICheckInService
{
    public async Task<CheckInResultDto> CheckInAsync(string token, string? sourceIp, string? userAgent, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return await RecordFailureAsync("Token is required.", null, null, sourceIp, userAgent, cancellationToken);
        }

        try
        {
            var principal = jwtTokenService.ValidateCheckInToken(token);
            var userId = Guid.Parse(principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new SecurityTokenException("Token subject is missing."));
            var tokenId = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value ?? throw new SecurityTokenException("Token identifier is missing.");

            var user = await userRepository.GetByIdAsync(userId, cancellationToken);
            if (user is null || !user.IsActive)
            {
                return await RecordFailureAsync("Token is not associated with an active user.", userId, tokenId, sourceIp, userAgent, cancellationToken);
            }

            var currentToken = await userTokenRepository.GetCurrentByUserIdAsync(userId, cancellationToken);
            if (currentToken is null || !currentToken.IsCurrent || !string.Equals(currentToken.TokenId, tokenId, StringComparison.Ordinal))
            {
                return await RecordFailureAsync("Token has been revoked or superseded.", userId, tokenId, sourceIp, userAgent, cancellationToken);
            }

            if (currentToken.RevokedAtUtc.HasValue)
            {
                return await RecordFailureAsync("Token has been revoked.", userId, tokenId, sourceIp, userAgent, cancellationToken);
            }

            if (currentToken.ExpiresAtUtc <= DateTime.UtcNow)
            {
                return await RecordFailureAsync("Token has expired.", userId, tokenId, sourceIp, userAgent, cancellationToken);
            }

            var successRecord = new CheckInRecord
            {
                UserId = user.Id,
                UserTokenId = currentToken.Id,
                SubmittedTokenId = tokenId,
                Status = CheckInRecordStatus.Success,
                CheckedInAtUtc = DateTime.UtcNow,
                SourceIp = sourceIp,
                UserAgent = userAgent
            };

            await checkInRecordRepository.AddAsync(successRecord, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("User {UserName} checked in successfully.", user.UserName);
            return new CheckInResultDto
            {
                IsSuccessful = true,
                Status = "Success",
                Message = "Check-in successful.",
                CheckedInAtUtc = successRecord.CheckedInAtUtc
            };
        }
        catch (SecurityTokenExpiredException)
        {
            var ids = SafeReadIdentifiers(token);
            return await RecordFailureAsync("Token has expired.", ids.UserId, ids.TokenId, sourceIp, userAgent, cancellationToken);
        }
        catch (SecurityTokenException)
        {
            var ids = SafeReadIdentifiers(token);
            return await RecordFailureAsync("Token signature is invalid.", ids.UserId, ids.TokenId, sourceIp, userAgent, cancellationToken);
        }
        catch (ArgumentException)
        {
            var ids = SafeReadIdentifiers(token);
            return await RecordFailureAsync("Token format is invalid.", ids.UserId, ids.TokenId, sourceIp, userAgent, cancellationToken);
        }
    }

    public async Task<PagedResult<CheckInRecordDto>> GetRecordsAsync(CheckInRecordQueryDto query, CancellationToken cancellationToken = default)
    {
        var records = await checkInRecordRepository.GetPagedAsync(query.UserId, query.StartUtc, query.EndUtc, query.Status, query.PageNumber, query.PageSize, cancellationToken);
        var totalCount = await checkInRecordRepository.CountAsync(query.UserId, query.StartUtc, query.EndUtc, query.Status, cancellationToken);

        return new PagedResult<CheckInRecordDto>
        {
            Items = records.Select(x => x.ToDto()).ToArray(),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    private (Guid? UserId, string? TokenId) SafeReadIdentifiers(string token)
    {
        try
        {
            return jwtTokenService.ReadIdentifiers(token);
        }
        catch
        {
            return (null, null);
        }
    }

    private async Task<CheckInResultDto> RecordFailureAsync(string reason, Guid? userId, string? tokenId, string? sourceIp, string? userAgent, CancellationToken cancellationToken)
    {
        var tokenEntity = !string.IsNullOrWhiteSpace(tokenId)
            ? await userTokenRepository.GetByTokenIdAsync(tokenId, cancellationToken)
            : null;

        var record = new CheckInRecord
        {
            UserId = userId ?? tokenEntity?.UserId,
            UserTokenId = tokenEntity?.Id,
            SubmittedTokenId = tokenId,
            Status = CheckInRecordStatus.Failed,
            FailureReason = reason,
            CheckedInAtUtc = DateTime.UtcNow,
            SourceIp = sourceIp,
            UserAgent = userAgent
        };

        await checkInRecordRepository.AddAsync(record, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogWarning("Check-in failed. Reason: {Reason}", reason);
        return new CheckInResultDto
        {
            IsSuccessful = false,
            Status = "Failed",
            Message = "Check-in failed.",
            FailureReason = reason,
            CheckedInAtUtc = record.CheckedInAtUtc
        };
    }
}
