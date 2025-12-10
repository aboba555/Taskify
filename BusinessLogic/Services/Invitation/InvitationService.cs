using BusinessLogic.DTO;
using DataAccess;
using DataAccess.Enums;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services.Invitation;

public class InvitationService(AppDbContext appDbContext) : IInvitationService
{
    public async Task SendInvite(SendInviteDto sendInviteDto, int senderId)
    {
        var existTeam = await appDbContext.Teams
            .FirstOrDefaultAsync(x => x.Id == sendInviteDto.TeamId);
        
        if (existTeam == null)
        {
            throw new Exception("Team not found");
        }
        
        var sender = await appDbContext.TeamUsers.FirstOrDefaultAsync(x => x.UserId == senderId && x.TeamId == sendInviteDto.TeamId);

        if (sender.Role != Role.Leader || sender == null)
        {
            throw new Exception("You are not the leader of this team");
        }
        
        var invitedUser = await appDbContext.Users.FirstOrDefaultAsync(u => u.Email == sendInviteDto.Email);
        
        if (invitedUser != null)
        {
            var isAlreadyMember = await appDbContext.TeamUsers
                .AnyAsync(tu => tu.UserId == invitedUser.Id && tu.TeamId == sendInviteDto.TeamId);

            if (isAlreadyMember)
            {
                throw new Exception("User is already a member of this team");
            }
            if (invitedUser.Id == senderId)
            {
                throw new Exception("You cannot invite yourself");
            }
        }
        
        var invateExist = await appDbContext.Invitations
            .AnyAsync(i => i.TeamId == sendInviteDto.TeamId &&
                           i.Email == sendInviteDto.Email &&
                           i.Status == Status.Pending);

        if (invateExist)
        {
            throw new Exception("Invitation already sent");
        }
        
        var invitation = new DataAccess.Models.Invitation
        {
            Email = sendInviteDto.Email,
            TeamId = sendInviteDto.TeamId,
            SenderId = senderId,
            InvitedUserId = invitedUser?.Id,
            Status = Status.Pending,
            Created = DateTime.UtcNow
        };
        
        await appDbContext.Invitations.AddAsync(invitation);
        await appDbContext.SaveChangesAsync();
    }

    public async Task<List<InvitationDto>> GetAllInvites(int userId)
    {
        return await appDbContext.Invitations
            .Include(i => i.Team)
            .Include(i => i.Sender)
            .Where(i => i.InvitedUserId == userId && i.Status == Status.Pending)
            .Select(i=> new InvitationDto
            {
                Id = i.Id,
                TeamName = i.Team.Name,
                SenderName = $"{i.Sender.FirstName} {i.Sender.LastName}",
                SendedAt = i.Created
            })
            .ToListAsync();
    }
}