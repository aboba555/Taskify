using DataAccess;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BusinessLogic.Services.TelegramService;

public class TelegramService(AppDbContext appDbContext, IConfiguration configuration, TelegramBotClient telegramBotClient) : ITelegramService
{
    public async Task<string> GenerateTelegramLink(int userId)
    {
        var user = await appDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        string generatedCode = GenerateCode();
        
        var linkCode = new TelegramLinkCode
        {
            UserId = userId,
            Code = generatedCode,
            ExpiresAt = DateTime.UtcNow.AddMinutes(20),
            IsUsed = false
        };
        
        await appDbContext.TelegramLinkCodes.AddAsync(linkCode);
        await appDbContext.SaveChangesAsync();
        
        var botUsername = configuration["Telegram:BotUsername"];
        return $"tg://resolve?domain={botUsername}&start={generatedCode}";
    }

    public async Task ProcessWebhook(Update update)
    {
        if (update.Message?.Text == null)
        {
            return;
        }
        
        if (!update.Message.Text.StartsWith("/start "))
        {
            return;
        }
        
        var parts = update.Message.Text.Split(' ');
        if (parts.Length < 2)
        {
            return;
        }
        var codeFromText = parts[1];
        
        var code = await appDbContext.TelegramLinkCodes.FirstOrDefaultAsync(l => l.Code == codeFromText);
        if (code == null || code.ExpiresAt < DateTime.UtcNow || code.IsUsed)
        {
            return;
        }
        
        var user = await appDbContext.Users.FirstOrDefaultAsync(u => u.Id == code.UserId);
        if (user == null)
        {
            return;
        }
        
        code.IsUsed = true;
        user.TelegramChatId =  update.Message.Chat.Id.ToString();
        
        await appDbContext.SaveChangesAsync();
    }

    public async Task<bool> IsConnected(int userId)
    {
        var user = await appDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        return user.TelegramChatId != null;
    }

    public async Task Disconnect(int userId)
    {
        var user = await appDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        user.TelegramChatId = null;
        await appDbContext.SaveChangesAsync();
    }

    public async Task SendMessage(int userId, string message)
    {
        var user = await appDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.TelegramChatId == null)
        {
            return;
        }
        
        try
        {
            await telegramBotClient.SendMessage(
                chatId: user.TelegramChatId,
                text: message,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Telegram send error: {ex.Message}");
        }
        
    }

    private string GenerateCode()
    {
        return Guid.NewGuid().ToString("N")[..24];
    }
}