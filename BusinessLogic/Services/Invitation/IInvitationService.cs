using BusinessLogic.DTO;

namespace BusinessLogic.Services.Invitation;

public interface IInvitationService
{
    Task SendInvite(SendInviteDto sendInviteDto, int senderId);
    Task<List<InvitationDto>> GetAllInvites(int userId);
    Task DeclineInvite(int inviteId, int userId);
    Task AcceptInvite(int inviteId, int userId);
}