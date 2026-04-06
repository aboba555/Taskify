using System.Threading.Channels;
using BusinessLogic.Services.TelegramService;
using DataAccess;
using DataAccess.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BusinessLogic.Services.NotificationWorker;

public class NotificationWorker(Channel<Notification> channel, IServiceScopeFactory scopeFactory) : BackgroundService
{
    private async Task ProcessNotifications(int workerId, CancellationToken stoppingToken)
    {
        Console.WriteLine($"{workerId} started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var notification = await channel.Reader.ReadAsync(stoppingToken);
                
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var telegram = scope.ServiceProvider.GetRequiredService<ITelegramService>();

                await db.Notifications.AddAsync(notification);
                await db.SaveChangesAsync(stoppingToken);
                
                var user = await db.Users.FindAsync(notification.ToUserId);
                if (user?.TelegramChatId != null)
                {
                    await telegram.SendMessage(user.Id, notification.Text);
                }
            }
            catch (OperationCanceledException)
            {
                break; // normal when shutdown
            }
            catch (Exception e)
            {
                Console.WriteLine($"Worker {workerId} error: {e.Message}");
            }
        }
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var workers = new[]
        {
            ProcessNotifications(1, stoppingToken),
            ProcessNotifications(2, stoppingToken)
        };
        return Task.WhenAll(workers);
    }
}