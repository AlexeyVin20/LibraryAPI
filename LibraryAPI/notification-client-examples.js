/**
 * ИСПРАВЛЕНИЕ ОШИБКИ 401 В МЕТОДАХ УВЕДОМЛЕНИЙ
 * 
 * Все методы API уведомлений требуют Bearer токен в заголовке Authorization.
 * Этот файл содержит правильные примеры клиентского кода.
 */

// ============================================================================
// 1. БАЗОВАЯ ФУНКЦИЯ ДЛЯ API ЗАПРОСОВ С BEARER ТОКЕНОМ
// ============================================================================

/**
 * Получение JWT токена из localStorage или cookies
 */
const getAuthToken = () => {
    // Попробовать получить из localStorage
    let token = localStorage.getItem('jwt_token');
    
    // Если нет в localStorage, попробовать из cookies
    if (!token) {
        const cookieValue = document.cookie
            .split('; ')
            .find(row => row.startsWith('token='))
            ?.split('=')[1];
        token = cookieValue;
    }
    
    return token;
};

/**
 * Базовая функция для всех API запросов с автоматическим добавлением Bearer токена
 */
const apiRequest = async (url, options = {}) => {
    const token = getAuthToken();
    
    if (!token) {
        console.error('JWT токен не найден');
        window.location.href = '/login';
        return;
    }
    
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
    
    try {
        const response = await fetch(url, config);
        
        if (response.status === 401) {
            console.error('Ошибка авторизации: токен недействителен или истек');
            localStorage.removeItem('jwt_token');
            // Удалить токен из cookies
            document.cookie = 'token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
            window.location.href = '/login';
            return;
        }
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        return response.json();
    } catch (error) {
        console.error('Ошибка API запроса:', error);
        throw error;
    }
};

// ============================================================================
// 2. МЕТОДЫ УВЕДОМЛЕНИЙ С ПРАВИЛЬНЫМИ BEARER ТОКЕНАМИ
// ============================================================================

/**
 * 1. Получение уведомлений пользователя
 * GET /api/notification?isRead=false&page=1&pageSize=20
 */
const getUserNotifications = async (isRead = null, page = 1, pageSize = 20) => {
    const params = new URLSearchParams();
    if (isRead !== null) params.append('isRead', isRead.toString());
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());
    
    return await apiRequest(`/api/notification?${params}`);
};

/**
 * 2. Получение статистики уведомлений
 * GET /api/notification/stats
 */
const getNotificationStats = async () => {
    return await apiRequest('/api/notification/stats');
};

/**
 * 3. Получение количества непрочитанных уведомлений
 * GET /api/notification/unread-count
 */
const getUnreadCount = async () => {
    return await apiRequest('/api/notification/unread-count');
};

/**
 * 4. Отметка уведомления как прочитанного
 * PUT /api/notification/{notificationId}/mark-read
 */
const markNotificationAsRead = async (notificationId) => {
    return await apiRequest(`/api/notification/${notificationId}/mark-read`, {
        method: 'PUT'
    });
};

/**
 * 5. Отметка нескольких уведомлений как прочитанных
 * PUT /api/notification/mark-multiple-read
 */
const markMultipleAsRead = async (notificationIds) => {
    return await apiRequest('/api/notification/mark-multiple-read', {
        method: 'PUT',
        body: JSON.stringify({ notificationIds })
    });
};

/**
 * 6. Отметка всех уведомлений как прочитанных
 * PUT /api/notification/mark-all-read
 */
const markAllAsRead = async () => {
    return await apiRequest('/api/notification/mark-all-read', {
        method: 'PUT'
    });
};

/**
 * 7. Отправка индивидуального уведомления (только для Admin/Librarian)
 * POST /api/notification/send
 */
const sendNotification = async (notificationData) => {
    return await apiRequest('/api/notification/send', {
        method: 'POST',
        body: JSON.stringify(notificationData)
    });
};

/**
 * 8. Отправка массовых уведомлений (только для Admin/Librarian)
 * POST /api/notification/send-bulk
 */
const sendBulkNotification = async (bulkData) => {
    return await apiRequest('/api/notification/send-bulk', {
        method: 'POST',
        body: JSON.stringify(bulkData)
    });
};

/**
 * 9. Отправка напоминаний о возврате книг (только для Admin/Librarian)
 * POST /api/notification/send-due-reminders
 */
const sendDueReminders = async () => {
    return await apiRequest('/api/notification/send-due-reminders', {
        method: 'POST'
    });
};

/**
 * 10. Отправка уведомлений о просроченных книгах (только для Admin/Librarian)
 * POST /api/notification/send-overdue-notifications
 */
const sendOverdueNotifications = async () => {
    return await apiRequest('/api/notification/send-overdue-notifications', {
        method: 'POST'
    });
};

/**
 * 11. Отправка уведомлений о штрафах (только для Admin/Librarian)
 * POST /api/notification/send-fine-notifications
 */
const sendFineNotifications = async () => {
    return await apiRequest('/api/notification/send-fine-notifications', {
        method: 'POST'
    });
};

// ============================================================================
// 3. AXIOS ПОДХОД (РЕКОМЕНДУЕМЫЙ)
// ============================================================================

// Если используете axios, создайте инстанс с перехватчиками:
/*
import axios from 'axios';

const api = axios.create({
    baseURL: '/api',
    headers: {
        'Content-Type': 'application/json'
    }
});

// Перехватчик запросов - автоматически добавляет Bearer токен
api.interceptors.request.use(
    (config) => {
        const token = getAuthToken();
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
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
    getNotifications: (isRead, page, pageSize) => 
        api.get('/notification', { params: { isRead, page, pageSize } }),
    getStats: () => 
        api.get('/notification/stats'),
    getUnreadCount: () => 
        api.get('/notification/unread-count'),
    markAsRead: (id) => 
        api.put(`/notification/${id}/mark-read`),
    markMultipleAsRead: (ids) => 
        api.put('/notification/mark-multiple-read', { notificationIds: ids }),
    markAllAsRead: () => 
        api.put('/notification/mark-all-read'),
    sendNotification: (data) => 
        api.post('/notification/send', data),
    sendBulkNotification: (data) => 
        api.post('/notification/send-bulk', data),
    sendDueReminders: () => 
        api.post('/notification/send-due-reminders'),
    sendOverdueNotifications: () => 
        api.post('/notification/send-overdue-notifications'),
    sendFineNotifications: () => 
        api.post('/notification/send-fine-notifications')
};
*/

// ============================================================================
// 4. ПРИМЕРЫ ИСПОЛЬЗОВАНИЯ
// ============================================================================

/**
 * Пример использования в React компоненте
 */
const NotificationComponent = () => {
    const [notifications, setNotifications] = useState([]);
    const [unreadCount, setUnreadCount] = useState(0);
    const [loading, setLoading] = useState(false);

    // Загрузка уведомлений
    const loadNotifications = async () => {
        try {
            setLoading(true);
            const data = await getUserNotifications();
            setNotifications(data);
        } catch (error) {
            console.error('Ошибка загрузки уведомлений:', error);
        } finally {
            setLoading(false);
        }
    };

    // Загрузка количества непрочитанных
    const loadUnreadCount = async () => {
        try {
            const data = await getUnreadCount();
            setUnreadCount(data.unreadCount);
        } catch (error) {
            console.error('Ошибка загрузки количества:', error);
        }
    };

    // Отметка как прочитанного
    const handleMarkAsRead = async (notificationId) => {
        try {
            await markNotificationAsRead(notificationId);
            await loadNotifications();
            await loadUnreadCount();
        } catch (error) {
            console.error('Ошибка отметки уведомления:', error);
        }
    };

    // Отметка всех как прочитанных
    const handleMarkAllAsRead = async () => {
        try {
            await markAllAsRead();
            await loadNotifications();
            await loadUnreadCount();
        } catch (error) {
            console.error('Ошибка отметки всех уведомлений:', error);
        }
    };

    useEffect(() => {
        loadNotifications();
        loadUnreadCount();
    }, []);

    return (
        <div>
            <h2>Уведомления ({unreadCount} непрочитанных)</h2>
            {loading ? (
                <p>Загрузка...</p>
            ) : (
                <div>
                    <button onClick={handleMarkAllAsRead}>
                        Отметить все как прочитанные
                    </button>
                    {notifications.map(notification => (
                        <div key={notification.id}>
                            <h3>{notification.title}</h3>
                            <p>{notification.message}</p>
                            {!notification.isRead && (
                                <button onClick={() => handleMarkAsRead(notification.id)}>
                                    Отметить как прочитанное
                                </button>
                            )}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};

// ============================================================================
// 5. ПРОВЕРКА ТОКЕНА ПЕРЕД ЗАПРОСАМИ
// ============================================================================

/**
 * Утилита для проверки валидности JWT токена
 */
const isTokenValid = (token) => {
    if (!token) return false;
    
    try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const currentTime = Date.now() / 1000;
        return payload.exp > currentTime;
    } catch (error) {
        console.error('Ошибка парсинга токена:', error);
        return false;
    }
};

/**
 * Функция для получения действительного токена с автообновлением
 */
const getValidToken = async () => {
    let token = getAuthToken();
    
    if (!isTokenValid(token)) {
        console.log('Токен истек, попытка обновления...');
        
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
                    console.log('Токен успешно обновлен');
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
            console.log('Refresh токен отсутствует, перенаправление на логин');
            window.location.href = '/login';
            return null;
        }
    }
    
    return token;
};

// ============================================================================
// 6. ЭКСПОРТ ФУНКЦИЙ
// ============================================================================

// Если используете модули ES6:
export {
    getUserNotifications,
    getNotificationStats,
    getUnreadCount,
    markNotificationAsRead,
    markMultipleAsRead,
    markAllAsRead,
    sendNotification,
    sendBulkNotification,
    sendDueReminders,
    sendOverdueNotifications,
    sendFineNotifications,
    getAuthToken,
    isTokenValid,
    getValidToken
};

// Если используете CommonJS:
/*
module.exports = {
    getUserNotifications,
    getNotificationStats,
    getUnreadCount,
    markNotificationAsRead,
    markMultipleAsRead,
    markAllAsRead,
    sendNotification,
    sendBulkNotification,
    sendDueReminders,
    sendOverdueNotifications,
    sendFineNotifications,
    getAuthToken,
    isTokenValid,
    getValidToken
};
*/ 