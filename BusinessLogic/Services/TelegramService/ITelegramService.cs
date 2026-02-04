using Telegram.Bot.Types;

namespace BusinessLogic.Services.TelegramService;

public interface ITelegramService
{
    Task<string> GenerateTelegramLink(int userId);
    Task ProcessWebhook(Update update);
    Task<bool> IsConnected(int userId);
    Task Disconnect(int userId);
}