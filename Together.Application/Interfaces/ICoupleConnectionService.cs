using Together.Application.DTOs;

namespace Together.Application.Interfaces;

public interface ICoupleConnectionService
{
    Task<ConnectionRequestDto> SendConnectionRequestAsync(Guid fromUserId, Guid toUserId);
    Task<CoupleConnectionDto> AcceptConnectionRequestAsync(Guid requestId, Guid userId);
    Task RejectConnectionRequestAsync(Guid requestId, Guid userId);
    Task TerminateConnectionAsync(Guid connectionId, Guid userId);
    Task<CoupleConnectionDto?> GetUserConnectionAsync(Guid userId);
    Task<IEnumerable<ConnectionRequestDto>> GetPendingRequestsAsync(Guid userId);
}
