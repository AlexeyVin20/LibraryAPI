# Система Push Уведомлений для Библиотечного API

## Обзор

Система push уведомлений предоставляет real-time уведомления для пользователей библиотеки с использованием SignalR для веб-приложений и REST API для мобильных приложений.

## Новые API Эндпоинты

### 1. Пользователи с книгами на руках
```
GET /api/users/with-books
```
- Возвращает всех пользователей с взятыми книгами
- Автоматически отправляет напоминания о скором возврате
- Включает информацию о:
  - Названии и авторах книг
  - Дате выдачи и сроке возврата
  - Статусе (активна/просрочена)
  - Количестве дней до/после срока возврата

### 2. Пользователи с задолженностями
```
GET /api/users/with-fines
```
- Возвращает пользователей со штрафами > 0
- Автоматически отправляет уведомления о штрафах
- Включает информацию о:
  - Сумме штрафа
  - Просроченных книгах
  - Предполагаемом штрафе за каждую просроченную книгу
  - Причинах штрафа

## Система Уведомлений

### Типы уведомлений
- `BookDueSoon` - Книга скоро должна быть возвращена
- `BookOverdue` - Книга просрочена
- `FineAdded` - Добавлен штраф
- `FineIncreased` - Штраф увеличен
- `BookReturned` - Книга возвращена
- `BookReserved` - Книга зарезервирована
- `ReservationExpired` - Бронь истекла
- `NewBookAvailable` - Новая книга доступна
- `AccountBlocked` - Аккаунт заблокирован
- `SystemMaintenance` - Техническое обслуживание
- `GeneralInfo` - Общая информация

### Приоритеты уведомлений
- `Low` - Низкий
- `Normal` - Обычный
- `High` - Высокий
- `Critical` - Критический

## API Уведомлений

### Получение уведомлений пользователя
```http
GET /api/notification?isRead=false&page=1&pageSize=20
Authorization: Bearer {token}
```

### Получение статистики уведомлений
```http
GET /api/notification/stats
Authorization: Bearer {token}
```

### Получение количества непрочитанных уведомлений
```http
GET /api/notification/unread-count
Authorization: Bearer {token}
```

### Отметка уведомления как прочитанного
```http
PUT /api/notification/{notificationId}/mark-read
Authorization: Bearer {token}
```

### Отметка нескольких уведомлений как прочитанных
```http
PUT /api/notification/mark-multiple-read
Authorization: Bearer {token}
Content-Type: application/json

{
  "notificationIds": ["guid1", "guid2", "guid3"]
}
```

### Отметка всех уведомлений как прочитанных
```http
PUT /api/notification/mark-all-read
Authorization: Bearer {token}
```

## Административные эндпоинты

### Отправка индивидуального уведомления
```http
POST /api/notification/send
Authorization: Bearer {token}
Content-Type: application/json

{
  "userId": "user-guid",
  "title": "Заголовок уведомления",
  "message": "Текст уведомления",
  "type": "GeneralInfo",
  "priority": "Normal",
  "additionalData": "{\"key\": \"value\"}"
}
```

### Отправка массовых уведомлений
```http
POST /api/notification/send-bulk
Authorization: Bearer {token}
Content-Type: application/json

{
  "userIds": ["guid1", "guid2"],
  "title": "Массовое уведомление",
  "message": "Текст для всех пользователей",
  "type": "SystemMaintenance",
  "priority": "High"
}
```

### Отправка напоминаний о возврате книг
```http
POST /api/notification/send-due-reminders
Authorization: Bearer {token}
```

### Отправка уведомлений о просроченных книгах
```http
POST /api/notification/send-overdue-notifications
Authorization: Bearer {token}
```

### Отправка уведомлений о штрафах
```http
POST /api/notification/send-fine-notifications
Authorization: Bearer {token}
```

## SignalR Push Уведомления

### Подключение к Hub
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub", {
        accessTokenFactory: () => localStorage.getItem("jwt_token")
    })
    .build();

connection.start().then(function () {
    console.log("Подключен к NotificationHub");
}).catch(function (err) {
    console.error(err.toString());
});
```

### Получение уведомлений
```javascript
connection.on("ReceiveNotification", function (notification) {
    console.log("Новое уведомление:", notification);
    // notification содержит:
    // - Title: заголовок
    // - Message: текст
    // - Type: тип уведомления
    // - Timestamp: время отправки
    
    // Показать уведомление пользователю
    showNotification(notification);
});
```

### Подтверждение получения
```javascript
connection.invoke("ConfirmNotificationReceived", notificationId)
    .catch(function (err) {
        console.error(err.toString());
    });
```

## Автоматические проверки

Система автоматически выполняет проверки каждые 6 часов:

1. **Напоминания о возврате** - отправляются за 1-3 дня до срока возврата
2. **Уведомления о просрочке** - отправляются для всех просроченных книг
3. **Обновление штрафов** - расчет и начисление штрафов (10 руб/день)
4. **Очистка старых уведомлений** - удаление прочитанных уведомлений старше 90 дней

## Модели данных

### NotificationDto
```json
{
  "id": "guid",
  "userId": "guid",
  "title": "string",
  "message": "string",
  "type": "BookDueSoon",
  "priority": "High",
  "createdAt": "2024-01-01T00:00:00Z",
  "isRead": false,
  "readAt": null,
  "isDelivered": true,
  "deliveredAt": "2024-01-01T00:00:01Z",
  "additionalData": "{}",
  "bookId": "guid",
  "borrowedBookId": "guid",
  "bookTitle": "Название книги",
  "bookAuthors": "Автор",
  "bookCover": "url"
}
```

### NotificationStatsDto
```json
{
  "totalNotifications": 50,
  "unreadNotifications": 5,
  "deliveredNotifications": 45,
  "pendingNotifications": 5,
  "notificationsByType": {
    "BookDueSoon": 10,
    "BookOverdue": 5,
    "FineAdded": 3
  },
  "notificationsByPriority": {
    "High": 8,
    "Normal": 30,
    "Critical": 2
  }
}
```

## Интеграция в существующие контроллеры

Система автоматически интегрирована в:

- **UserController.GetUsersWithBooks()** - отправляет напоминания при каждом запросе
- **UserController.GetUsersWithFines()** - отправляет уведомления о штрафах при каждом запросе

## Настройка и развертывание

### Требования
- .NET 6.0+
- PostgreSQL
- SignalR
- Entity Framework Core

### Настройка в Program.cs
```csharp
// Сервисы уведомлений
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHostedService<NotificationBackgroundService>();

// SignalR
builder.Services.AddSignalR();

// Маршруты
app.MapHub<NotificationHub>("/notificationHub");
```

### Миграция базы данных
```bash
dotnet ef migrations add AddNotifications
dotnet ef database update
```

## Безопасность

- Все эндпоинты требуют аутентификации
- Административные функции требуют роли Admin или Librarian
- SignalR Hub использует JWT токены для аутентификации
- Пользователи получают только свои уведомления

## Мониторинг и логирование

Система логирует:
- Создание и отправку уведомлений
- Ошибки доставки push уведомлений
- Статистику работы фонового сервиса
- Очистку старых уведомлений

## Производительность

- Использование индексов для быстрого поиска по пользователю и дате
- Пагинация для больших списков уведомлений
- Асинхронная отправка push уведомлений
- Автоматическая очистка старых данных

## Примеры использования

### React/JavaScript клиент
```javascript
// Подключение к SignalR
const notificationService = {
    connection: null,
    
    async connect(token) {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub", {
                accessTokenFactory: () => token
            })
            .build();
            
        await this.connection.start();
        this.setupHandlers();
    },
    
    setupHandlers() {
        this.connection.on("ReceiveNotification", (notification) => {
            // Показать toast уведомление
            toast.info(notification.Message, {
                title: notification.Title,
                type: notification.Type
            });
            
            // Обновить счетчик непрочитанных
            this.updateUnreadCount();
        });
    },
    
    async getNotifications(page = 1) {
        const response = await fetch(`/api/notification?page=${page}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        return await response.json();
    },
    
    async markAsRead(notificationId) {
        await fetch(`/api/notification/${notificationId}/mark-read`, {
            method: 'PUT',
            headers: { 'Authorization': `Bearer ${token}` }
        });
    }
};
```

### Правильные примеры клиентских запросов с Bearer токеном

#### JavaScript/TypeScript с fetch API
```javascript
// Получение токена из localStorage или cookies
const getAuthToken = () => {
    return localStorage.getItem('jwt_token') || 
           document.cookie.split('; ').find(row => row.startsWith('token='))?.split('=')[1];
};

// Базовая функция для API запросов
const apiRequest = async (url, options = {}) => {
    const token = getAuthToken();
    
    const defaultHeaders = {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
    };
    
    const config = {
        ...options,
        headers: {
            ...defaultHeaders,
            ...options.headers
        }
    };
    
    const response = await fetch(url, config);
    
    if (response.status === 401) {
        // Токен истек или недействителен
        console.error('Ошибка авторизации: токен недействителен');
        // Перенаправить на страницу входа
        window.location.href = '/login';
        return;
    }
    
    if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
    }
    
    return response.json();
};

// Примеры использования для всех методов уведомлений

// 1. Получение уведомлений пользователя
const getUserNotifications = async (isRead = null, page = 1, pageSize = 20) => {
    const params = new URLSearchParams();
    if (isRead !== null) params.append('isRead', isRead);
    params.append('page', page);
    params.append('pageSize', pageSize);
    
    return await apiRequest(`/api/notification?${params}`);
};

// 2. Получение статистики уведомлений
const getNotificationStats = async () => {
    return await apiRequest('/api/notification/stats');
};

// 3. Получение количества непрочитанных уведомлений
const getUnreadCount = async () => {
    return await apiRequest('/api/notification/unread-count');
};

// 4. Отметка уведомления как прочитанного
const markNotificationAsRead = async (notificationId) => {
    return await apiRequest(`/api/notification/${notificationId}/mark-read`, {
        method: 'PUT'
    });
};

// 5. Отметка нескольких уведомлений как прочитанных
const markMultipleAsRead = async (notificationIds) => {
    return await apiRequest('/api/notification/mark-multiple-read', {
        method: 'PUT',
        body: JSON.stringify({ notificationIds })
    });
};

// 6. Отметка всех уведомлений как прочитанных
const markAllAsRead = async () => {
    return await apiRequest('/api/notification/mark-all-read', {
        method: 'PUT'
    });
};

// 7. Отправка индивидуального уведомления (только для Admin/Librarian)
const sendNotification = async (notificationData) => {
    return await apiRequest('/api/notification/send', {
        method: 'POST',
        body: JSON.stringify(notificationData)
    });
};

// 8. Отправка массовых уведомлений (только для Admin/Librarian)
const sendBulkNotification = async (bulkData) => {
    return await apiRequest('/api/notification/send-bulk', {
        method: 'POST',
        body: JSON.stringify(bulkData)
    });
};

// 9. Отправка напоминаний о возврате книг (только для Admin/Librarian)
const sendDueReminders = async () => {
    return await apiRequest('/api/notification/send-due-reminders', {
        method: 'POST'
    });
};

// 10. Отправка уведомлений о просроченных книгах (только для Admin/Librarian)
const sendOverdueNotifications = async () => {
    return await apiRequest('/api/notification/send-overdue-notifications', {
        method: 'POST'
    });
};

// 11. Отправка уведомлений о штрафах (только для Admin/Librarian)
const sendFineNotifications = async () => {
    return await apiRequest('/api/notification/send-fine-notifications', {
        method: 'POST'
    });
};
```

#### Axios с перехватчиками (рекомендуемый подход)
```javascript
import axios from 'axios';

// Создание инстанса axios
const api = axios.create({
    baseURL: '/api',
    headers: {
        'Content-Type': 'application/json'
    }
});

// Перехватчик запросов - автоматически добавляет Bearer токен
api.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('jwt_token') || 
                     document.cookie.split('; ').find(row => row.startsWith('token='))?.split('=')[1];
        
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

// Перехватчик ответов - обработка ошибок 401
api.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            console.error('Ошибка авторизации: токен недействителен');
            localStorage.removeItem('jwt_token');
            window.location.href = '/login';
        }
        return Promise.reject(error);
    }
);

// Сервис для работы с уведомлениями
const notificationService = {
    // Получение уведомлений
    async getNotifications(isRead = null, page = 1, pageSize = 20) {
        const params = { page, pageSize };
        if (isRead !== null) params.isRead = isRead;
        
        const response = await api.get('/notification', { params });
        return response.data;
    },

    // Получение статистики
    async getStats() {
        const response = await api.get('/notification/stats');
        return response.data;
    },

    // Получение количества непрочитанных
    async getUnreadCount() {
        const response = await api.get('/notification/unread-count');
        return response.data;
    },

    // Отметка как прочитанного
    async markAsRead(notificationId) {
        const response = await api.put(`/notification/${notificationId}/mark-read`);
        return response.data;
    },

    // Отметка нескольких как прочитанных
    async markMultipleAsRead(notificationIds) {
        const response = await api.put('/notification/mark-multiple-read', { notificationIds });
        return response.data;
    },

    // Отметка всех как прочитанных
    async markAllAsRead() {
        const response = await api.put('/notification/mark-all-read');
        return response.data;
    },

    // Административные методы
    async sendNotification(data) {
        const response = await api.post('/notification/send', data);
        return response.data;
    },

    async sendBulkNotification(data) {
        const response = await api.post('/notification/send-bulk', data);
        return response.data;
    },

    async sendDueReminders() {
        const response = await api.post('/notification/send-due-reminders');
        return response.data;
    },

    async sendOverdueNotifications() {
        const response = await api.post('/notification/send-overdue-notifications');
        return response.data;
    },

    async sendFineNotifications() {
        const response = await api.post('/notification/send-fine-notifications');
        return response.data;
    }
};

export default notificationService;
```

#### React Hook для уведомлений
```javascript
import { useState, useEffect, useCallback } from 'react';
import notificationService from '../services/notificationService';

export const useNotifications = () => {
    const [notifications, setNotifications] = useState([]);
    const [unreadCount, setUnreadCount] = useState(0);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    // Загрузка уведомлений
    const loadNotifications = useCallback(async (page = 1, pageSize = 20) => {
        try {
            setLoading(true);
            setError(null);
            const data = await notificationService.getNotifications(null, page, pageSize);
            setNotifications(data);
        } catch (err) {
            setError(err.message);
            console.error('Ошибка загрузки уведомлений:', err);
        } finally {
            setLoading(false);
        }
    }, []);

    // Загрузка количества непрочитанных
    const loadUnreadCount = useCallback(async () => {
        try {
            const data = await notificationService.getUnreadCount();
            setUnreadCount(data.unreadCount);
        } catch (err) {
            console.error('Ошибка загрузки количества непрочитанных:', err);
        }
    }, []);

    // Отметка как прочитанного
    const markAsRead = useCallback(async (notificationId) => {
        try {
            await notificationService.markAsRead(notificationId);
            await loadNotifications();
            await loadUnreadCount();
        } catch (err) {
            setError(err.message);
            console.error('Ошибка отметки уведомления:', err);
        }
    }, [loadNotifications, loadUnreadCount]);

    // Отметка всех как прочитанных
    const markAllAsRead = useCallback(async () => {
        try {
            await notificationService.markAllAsRead();
            await loadNotifications();
            await loadUnreadCount();
        } catch (err) {
            setError(err.message);
            console.error('Ошибка отметки всех уведомлений:', err);
        }
    }, [loadNotifications, loadUnreadCount]);

    useEffect(() => {
        loadNotifications();
        loadUnreadCount();
    }, [loadNotifications, loadUnreadCount]);

    return {
        notifications,
        unreadCount,
        loading,
        error,
        loadNotifications,
        loadUnreadCount,
        markAsRead,
        markAllAsRead
    };
};
```

#### Проверка токена перед запросами
```javascript
// Утилита для проверки валидности токена
const isTokenValid = (token) => {
    if (!token) return false;
    
    try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const currentTime = Date.now() / 1000;
        return payload.exp > currentTime;
    } catch (error) {
        return false;
    }
};

// Функция для получения действительного токена
const getValidToken = async () => {
    let token = localStorage.getItem('jwt_token');
    
    if (!isTokenValid(token)) {
        // Попытка обновить токен
        const refreshToken = localStorage.getItem('refresh_token');
        if (refreshToken) {
            try {
                const response = await fetch('/api/auth/refresh', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ refreshToken })
                });
                
                if (response.ok) {
                    const data = await response.json();
                    localStorage.setItem('jwt_token', data.token);
                    localStorage.setItem('refresh_token', data.refreshToken);
                    token = data.token;
                } else {
                    throw new Error('Не удалось обновить токен');
                }
            } catch (error) {
                console.error('Ошибка обновления токена:', error);
                localStorage.removeItem('jwt_token');
                localStorage.removeItem('refresh_token');
                window.location.href = '/login';
                return null;
            }
        } else {
            window.location.href = '/login';
            return null;
        }
    }
    
    return token;
};
```

### Mobile API интеграция
```csharp
// Сервис для мобильного приложения
public class MobileNotificationService
{
    private readonly HttpClient _httpClient;
    
    public async Task<List<NotificationDto>> GetNotificationsAsync()
    {
        var response = await _httpClient.GetAsync("/api/notification");
        return await response.Content.ReadFromJsonAsync<List<NotificationDto>>();
    }
    
    public async Task MarkAsReadAsync(Guid notificationId)
    {
        await _httpClient.PutAsync($"/api/notification/{notificationId}/mark-read", null);
    }
}
```

## Расширения и настройки

Систему можно легко расширить:

1. **Новые типы уведомлений** - добавить в enum NotificationType
2. **Email/SMS уведомления** - интегрировать SMTP/SMS провайдеры
3. **Push уведомления для мобильных** - интегрировать Firebase/APNS
4. **Шаблоны уведомлений** - создать систему шаблонов для сообщений
5. **Настройки пользователя** - добавить предпочтения по типам уведомлений 