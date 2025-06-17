using LibraryAPI.Services;

namespace LibraryAPI.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationBackgroundService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(6); // Проверка каждые 6 часов

        public NotificationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<NotificationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoWorkAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка в фоновом сервисе уведомлений");
                }

                await Task.Delay(_period, stoppingToken);
            }
        }

        private async Task DoWorkAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            _logger.LogInformation("Запуск автоматических проверок уведомлений");

            // Проверка и отправка напоминаний о возврате книг
            await notificationService.CheckAndSendDueRemindersAsync();

            // Проверка и отправка уведомлений о просроченных книгах
            await notificationService.CheckAndSendOverdueNotificationsAsync();

            // Обновление штрафов
            await notificationService.CheckAndUpdateFinesAsync();

            // Очистка старых уведомлений (раз в день)
            if (DateTime.UtcNow.Hour == 2) // В 2:00 ночи
            {
                await notificationService.CleanupOldNotificationsAsync();
            }

            _logger.LogInformation("Автоматические проверки уведомлений завершены");
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Остановка фонового сервиса уведомлений");
            await base.StopAsync(stoppingToken);
        }
    }
} 