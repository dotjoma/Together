using Together.Application.DTOs;
using Together.Application.Exceptions;
using Together.Application.Interfaces;
using Together.Domain.Entities;
using Together.Domain.Enums;
using Together.Domain.Interfaces;

namespace Together.Application.Services;

public class CoupleConnectionService : ICoupleConnectionService
{
    private readonly ICoupleConnectionRepository _connectionRepository;
    private readonly IConnectionRequestRepository _requestRepository;
    private readonly IUserRepository _userRepository;

    public CoupleConnectionService(
        ICoupleConnectionRepository connectionRepository,
        IConnectionRequestRepository requestRepository,
        IUserRepository userRepository)
    {
        _connectionRepository = connectionRepository;
        _requestRepository = requestRepository;
        _userRepository = userRepository;
    }

    public async Task<ConnectionRequestDto> SendConnectionRequestAsync(Guid fromUserId, Guid toUserId)
    {
        // Validate users exist
        var fromUser = await _userRepository.GetByIdAsync(fromUserId)
            ?? throw new NotFoundException(nameof(User), fromUserId);
        
        var toUser = await _userRepository.GetByIdAsync(toUserId)
            ?? throw new NotFoundException(nameof(User), toUserId);

        // Check if sender already has an active connection
        var existingConnection = await _connectionRepository.GetByUserIdAsync(fromUserId);
        if (existingConnection != null)
        {
            throw new BusinessRuleViolationException("You already have an active couple connection");
        }

        // Check if recipient already has an active connection
        var recipientConnection = await _connectionRepository.GetByUserIdAsync(toUserId);
        if (recipientConnection != null)
        {
            throw new BusinessRuleViolationException("The user you're trying to connect with already has an active couple connection");
        }

        // Check if there's already a pending request between these users
        var existingRequest = await _requestRepository.GetPendingRequestBetweenUsersAsync(fromUserId, toUserId);
        if (existingRequest != null)
        {
            throw new BusinessRuleViolationException("A connection request already exists between these users");
        }

        // Check for reverse pending request
        var reverseRequest = await _requestRepository.GetPendingRequestBetweenUsersAsync(toUserId, fromUserId);
        if (reverseRequest != null)
        {
            throw new BusinessRuleViolationException("This user has already sent you a connection request. Please respond to their request instead.");
        }

        // Create the connection request
        var request = new ConnectionRequest(fromUserId, toUserId);
        await _requestRepository.AddAsync(request);

        // Reload to get navigation properties
        var savedRequest = await _requestRepository.GetByIdAsync(request.Id);

        return MapToConnectionRequestDto(savedRequest!);
    }

    public async Task<CoupleConnectionDto> AcceptConnectionRequestAsync(Guid requestId, Guid userId)
    {
        var request = await _requestRepository.GetByIdAsync(requestId)
            ?? throw new NotFoundException(nameof(ConnectionRequest), requestId);

        // Verify the user is the recipient of the request
        if (request.ToUserId != userId)
        {
            throw new BusinessRuleViolationException("You can only accept connection requests sent to you");
        }

        // Verify request is still pending
        if (request.Status != ConnectionRequestStatus.Pending)
        {
            throw new BusinessRuleViolationException("This connection request has already been responded to");
        }

        // Check if either user already has an active connection
        var fromUserConnection = await _connectionRepository.GetByUserIdAsync(request.FromUserId);
        if (fromUserConnection != null)
        {
            throw new BusinessRuleViolationException("The requesting user already has an active couple connection");
        }

        var toUserConnection = await _connectionRepository.GetByUserIdAsync(request.ToUserId);
        if (toUserConnection != null)
        {
            throw new BusinessRuleViolationException("You already have an active couple connection");
        }

        // Accept the request
        request.Accept();
        await _requestRepository.UpdateAsync(request);

        // Create the couple connection
        var connection = new CoupleConnection(
            request.FromUserId,
            request.ToUserId,
            DateTime.UtcNow.Date // Default to today, can be customized later
        );

        await _connectionRepository.AddAsync(connection);

        // Reload to get navigation properties
        var savedConnection = await _connectionRepository.GetByIdAsync(connection.Id);

        return MapToCoupleConnectionDto(savedConnection!);
    }

    public async Task RejectConnectionRequestAsync(Guid requestId, Guid userId)
    {
        var request = await _requestRepository.GetByIdAsync(requestId)
            ?? throw new NotFoundException(nameof(ConnectionRequest), requestId);

        // Verify the user is the recipient of the request
        if (request.ToUserId != userId)
        {
            throw new BusinessRuleViolationException("You can only reject connection requests sent to you");
        }

        // Verify request is still pending
        if (request.Status != ConnectionRequestStatus.Pending)
        {
            throw new BusinessRuleViolationException("This connection request has already been responded to");
        }

        // Reject the request
        request.Reject();
        await _requestRepository.UpdateAsync(request);
    }

    public async Task TerminateConnectionAsync(Guid connectionId, Guid userId)
    {
        var connection = await _connectionRepository.GetByIdAsync(connectionId)
            ?? throw new NotFoundException(nameof(CoupleConnection), connectionId);

        // Verify the user is part of the connection
        if (connection.User1Id != userId && connection.User2Id != userId)
        {
            throw new BusinessRuleViolationException("You can only terminate your own couple connection");
        }

        // Verify connection is active
        if (connection.Status != ConnectionStatus.Active)
        {
            throw new BusinessRuleViolationException("This connection is not active");
        }

        // Terminate the connection (archives shared data)
        connection.Terminate();
        await _connectionRepository.UpdateAsync(connection);
    }

    public async Task<CoupleConnectionDto?> GetUserConnectionAsync(Guid userId)
    {
        var connection = await _connectionRepository.GetByUserIdAsync(userId);
        return connection != null ? MapToCoupleConnectionDto(connection) : null;
    }

    public async Task<IEnumerable<ConnectionRequestDto>> GetPendingRequestsAsync(Guid userId)
    {
        var requests = await _requestRepository.GetPendingRequestsForUserAsync(userId);
        return requests.Select(MapToConnectionRequestDto);
    }

    private static ConnectionRequestDto MapToConnectionRequestDto(ConnectionRequest request)
    {
        return new ConnectionRequestDto(
            request.Id,
            new UserDto(
                request.FromUser.Id,
                request.FromUser.Username,
                request.FromUser.Email.Value,
                request.FromUser.ProfilePictureUrl,
                request.FromUser.Bio
            ),
            new UserDto(
                request.ToUser.Id,
                request.ToUser.Username,
                request.ToUser.Email.Value,
                request.ToUser.ProfilePictureUrl,
                request.ToUser.Bio
            ),
            request.CreatedAt,
            request.Status.ToString()
        );
    }

    private static CoupleConnectionDto MapToCoupleConnectionDto(CoupleConnection connection)
    {
        return new CoupleConnectionDto(
            connection.Id,
            new UserDto(
                connection.User1.Id,
                connection.User1.Username,
                connection.User1.Email.Value,
                connection.User1.ProfilePictureUrl,
                connection.User1.Bio
            ),
            new UserDto(
                connection.User2.Id,
                connection.User2.Username,
                connection.User2.Email.Value,
                connection.User2.ProfilePictureUrl,
                connection.User2.Bio
            ),
            connection.EstablishedAt,
            connection.RelationshipStartDate,
            connection.LoveStreak,
            connection.LastInteractionDate,
            connection.Status.ToString()
        );
    }
}
