# Руководство по использованию JWT аутентификации с Next.js

Это руководство объясняет, как использовать реализованную JWT-аутентификацию из API в клиентском приложении Next.js.

## Содержание

1. [Установка необходимых зависимостей](#установка-необходимых-зависимостей)
2. [Типы данных](#типы-данных)
3. [Создание AuthContext](#создание-authcontext)
4. [Создание хуков и утилит для аутентификации](#создание-хуков-и-утилит-для-аутентификации)
5. [Интеграция с API](#интеграция-с-api)
6. [Защита маршрутов](#защита-маршрутов)
7. [Компоненты авторизации](#компоненты-авторизации)
8. [Пример использования](#пример-использования)

## Установка необходимых зависимостей

```bash
npm install axios js-cookie next-cookies
```

## Типы данных

Создайте файл `types/auth.ts`:

```typescript
// types/auth.ts
export interface User {
  id: string;
  username: string;
  fullName: string;
  email: string;
  roles: string[];
}

export interface AuthState {
  isAuthenticated: boolean;
  user: User | null;
  loading: boolean;
  error: string | null;
}

export interface LoginCredentials {
  username: string;
  password: string;
}

export interface RegisterCredentials {
  fullName: string;
  email: string;
  phone: string;
  dateOfBirth: string;
  passportNumber: string;
  passportIssuedBy: string;
  passportIssuedDate?: string;
  address: string;
  username: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}
```

## Создание AuthContext

Создайте файл `contexts/AuthContext.tsx`:

```typescript
// contexts/AuthContext.tsx
import React, { createContext, useReducer, useEffect } from 'react';
import Cookies from 'js-cookie';
import { useRouter } from 'next/router';
import { AuthState, LoginCredentials, RegisterCredentials, User } from '../types/auth';
import { loginUser, registerUser, refreshToken, getCurrentUser } from '../services/authService';

// Начальное состояние
const initialState: AuthState = {
  isAuthenticated: false,
  user: null,
  loading: true,
  error: null
};

// Типы действий
type AuthAction =
  | { type: 'LOGIN_SUCCESS'; payload: User }
  | { type: 'REGISTER_SUCCESS'; payload: User }
  | { type: 'AUTH_ERROR'; payload: string }
  | { type: 'LOGOUT' }
  | { type: 'CLEAR_ERROR' }
  | { type: 'SET_LOADING'; payload: boolean };

// Редуктор
const authReducer = (state: AuthState, action: AuthAction): AuthState => {
  switch (action.type) {
    case 'LOGIN_SUCCESS':
    case 'REGISTER_SUCCESS':
      return {
        ...state,
        isAuthenticated: true,
        loading: false,
        user: action.payload,
        error: null
      };
    case 'AUTH_ERROR':
      return {
        ...state,
        isAuthenticated: false,
        user: null,
        loading: false,
        error: action.payload
      };
    case 'LOGOUT':
      return {
        ...state,
        isAuthenticated: false,
        user: null,
        loading: false,
        error: null
      };
    case 'CLEAR_ERROR':
      return {
        ...state,
        error: null
      };
    case 'SET_LOADING':
      return {
        ...state,
        loading: action.payload
      };
    default:
      return state;
  }
};

// Создаем контекст
type AuthContextType = {
  state: AuthState;
  login: (credentials: LoginCredentials) => Promise<void>;
  register: (credentials: RegisterCredentials) => Promise<void>;
  logout: () => void;
  clearError: () => void;
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Провайдер контекста
export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [state, dispatch] = useReducer(authReducer, initialState);
  const router = useRouter();

  useEffect(() => {
    const loadUser = async () => {
      dispatch({ type: 'SET_LOADING', payload: true });
      
      const token = Cookies.get('token');
      if (!token) {
        dispatch({ type: 'SET_LOADING', payload: false });
        return;
      }

      try {
        const user = await getCurrentUser();
        dispatch({ type: 'LOGIN_SUCCESS', payload: user });
      } catch (error) {
        // Если токен недействителен, пробуем обновить его
        try {
          const refreshTokenValue = Cookies.get('refreshToken');
          if (refreshTokenValue) {
            const response = await refreshToken(refreshTokenValue);
            Cookies.set('token', response.token, { expires: new Date(response.expiresAt) });
            Cookies.set('refreshToken', response.refreshToken, { expires: 7 });
            dispatch({ type: 'LOGIN_SUCCESS', payload: response.user });
          } else {
            Cookies.remove('token');
            Cookies.remove('refreshToken');
            dispatch({ type: 'AUTH_ERROR', payload: 'Сессия истекла, пожалуйста, войдите снова.' });
          }
        } catch (refreshError) {
          Cookies.remove('token');
          Cookies.remove('refreshToken');
          dispatch({ type: 'AUTH_ERROR', payload: 'Сессия истекла, пожалуйста, войдите снова.' });
        }
      }
    };

    loadUser();
  }, []);

  const login = async (credentials: LoginCredentials) => {
    dispatch({ type: 'SET_LOADING', payload: true });
    try {
      const response = await loginUser(credentials);
      Cookies.set('token', response.token, { expires: new Date(response.expiresAt) });
      Cookies.set('refreshToken', response.refreshToken, { expires: 7 });
      dispatch({ type: 'LOGIN_SUCCESS', payload: response.user });
      router.push('/dashboard');
    } catch (error: any) {
      dispatch({
        type: 'AUTH_ERROR',
        payload: error.response?.data?.message || 'Ошибка входа. Проверьте данные и попробуйте снова.'
      });
    }
  };

  const register = async (credentials: RegisterCredentials) => {
    dispatch({ type: 'SET_LOADING', payload: true });
    try {
      const response = await registerUser(credentials);
      Cookies.set('token', response.token, { expires: new Date(response.expiresAt) });
      Cookies.set('refreshToken', response.refreshToken, { expires: 7 });
      dispatch({ type: 'REGISTER_SUCCESS', payload: response.user });
      router.push('/dashboard');
    } catch (error: any) {
      dispatch({
        type: 'AUTH_ERROR',
        payload: error.response?.data?.message || 'Ошибка регистрации. Проверьте данные и попробуйте снова.'
      });
    }
  };

  const logout = () => {
    Cookies.remove('token');
    Cookies.remove('refreshToken');
    dispatch({ type: 'LOGOUT' });
    router.push('/login');
  };

  const clearError = () => {
    dispatch({ type: 'CLEAR_ERROR' });
  };

  return (
    <AuthContext.Provider value={{ state, login, register, logout, clearError }}>
      {children}
    </AuthContext.Provider>
  );
};

export default AuthContext;
```

## Создание хуков и утилит для аутентификации

Создайте файл `hooks/useAuth.ts`:

```typescript
// hooks/useAuth.ts
import { useContext } from 'react';
import AuthContext from '../contexts/AuthContext';

const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

export default useAuth;
```

## Интеграция с API

Создайте файл `services/authService.ts`:

```typescript
// services/authService.ts
import axios from 'axios';
import Cookies from 'js-cookie';
import { LoginCredentials, RegisterCredentials, AuthResponse, User } from '../types/auth';

const API_URL = 'https://localhost:5000';

// Создание инстанса axios с перехватчиками
const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json'
  }
});

// Добавляем перехватчик для запросов
api.interceptors.request.use(
  config => {
    const token = Cookies.get('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  error => {
    return Promise.reject(error);
  }
);

// Добавляем перехватчик для ответов
api.interceptors.response.use(
  response => response,
  async error => {
    const originalRequest = error.config;

    // Если ошибка 401 (Unauthorized) и запрос еще не повторялся
    if (error.response.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshTokenValue = Cookies.get('refreshToken');
        if (!refreshTokenValue) {
          return Promise.reject(error);
        }

        const response = await axios.post<AuthResponse>(
          `${API_URL}/auth/refresh`,
          { refreshToken: refreshTokenValue },
          { headers: { 'Content-Type': 'application/json' } }
        );

        // Обновляем токены в куках
        Cookies.set('token', response.data.token, { expires: new Date(response.data.expiresAt) });
        Cookies.set('refreshToken', response.data.refreshToken, { expires: 7 });

        // Обновляем заголовок авторизации и повторяем исходный запрос
        originalRequest.headers.Authorization = `Bearer ${response.data.token}`;
        return api(originalRequest);
      } catch (refreshError) {
        // Если обновление токена не удалось, удаляем куки
        Cookies.remove('token');
        Cookies.remove('refreshToken');
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

// Функции для работы с API
export const loginUser = async (credentials: LoginCredentials): Promise<AuthResponse> => {
  const response = await api.post<AuthResponse>('/auth/login', credentials);
  return response.data;
};

export const registerUser = async (credentials: RegisterCredentials): Promise<AuthResponse> => {
  const response = await api.post<AuthResponse>('/auth/register', credentials);
  return response.data;
};

export const refreshToken = async (refreshTokenValue: string): Promise<AuthResponse> => {
  const response = await api.post<AuthResponse>('/auth/refresh', { refreshToken: refreshTokenValue });
  return response.data;
};

export const getCurrentUser = async (): Promise<User> => {
  const response = await api.get<User>('/auth/session');
  return response.data;
};

export default api;
```

## Защита маршрутов

Создайте HOC для защиты маршрутов `components/withAuth.tsx`:

```typescript
// components/withAuth.tsx
import { useRouter } from 'next/router';
import { useEffect } from 'react';
import useAuth from '../hooks/useAuth';

const withAuth = <P extends object>(
  Component: React.ComponentType<P>,
  requiredRoles: string[] = []
) => {
  const WithAuth = (props: P) => {
    const { state } = useAuth();
    const router = useRouter();

    useEffect(() => {
      if (!state.loading) {
        if (!state.isAuthenticated) {
          router.replace('/login');
        } else if (
          requiredRoles.length > 0 &&
          !requiredRoles.some(role => state.user?.roles.includes(role))
        ) {
          router.replace('/unauthorized');
        }
      }
    }, [state.loading, state.isAuthenticated, state.user, router]);

    if (state.loading) {
      return <div>Загрузка...</div>;
    }

    if (!state.isAuthenticated) {
      return null;
    }

    if (
      requiredRoles.length > 0 &&
      !requiredRoles.some(role => state.user?.roles.includes(role))
    ) {
      return null;
    }

    return <Component {...props} />;
  };

  WithAuth.displayName = `WithAuth(${Component.displayName || Component.name || 'Component'})`;
  return WithAuth;
};

export default withAuth;
```

## Компоненты авторизации

Пример компонента логина `components/LoginForm.tsx`:

```typescript
// components/LoginForm.tsx
import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import useAuth from '../hooks/useAuth';
import { LoginCredentials } from '../types/auth';

const LoginForm: React.FC = () => {
  const { register, handleSubmit, formState: { errors } } = useForm<LoginCredentials>();
  const { login, state } = useAuth();
  const [submitting, setSubmitting] = useState(false);

  const onSubmit = async (data: LoginCredentials) => {
    setSubmitting(true);
    await login(data);
    setSubmitting(false);
  };

  return (
    <div className="p-6 max-w-md mx-auto bg-white rounded-lg shadow-md">
      <h2 className="text-2xl font-bold mb-6 text-center">Вход в систему</h2>
      
      {state.error && (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative mb-4">
          {state.error}
        </div>
      )}
      
      <form onSubmit={handleSubmit(onSubmit)}>
        <div className="mb-4">
          <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="username">
            Имя пользователя
          </label>
          <input
            {...register('username', { required: 'Имя пользователя обязательно' })}
            className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            id="username"
            type="text"
            placeholder="Введите имя пользователя"
          />
          {errors.username && (
            <p className="text-red-500 text-xs italic">{errors.username.message}</p>
          )}
        </div>
        
        <div className="mb-6">
          <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="password">
            Пароль
          </label>
          <input
            {...register('password', { required: 'Пароль обязателен' })}
            className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            id="password"
            type="password"
            placeholder="Введите пароль"
          />
          {errors.password && (
            <p className="text-red-500 text-xs italic">{errors.password.message}</p>
          )}
        </div>
        
        <div className="flex items-center justify-between">
          <button
            type="submit"
            disabled={submitting}
            className={`bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline ${
              submitting ? 'opacity-50 cursor-not-allowed' : ''
            }`}
          >
            {submitting ? 'Вход...' : 'Войти'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default LoginForm;
```

## Пример использования

Создайте `_app.tsx` для обертывания приложения в AuthProvider:

```typescript
// pages/_app.tsx
import { AppProps } from 'next/app';
import { AuthProvider } from '../contexts/AuthContext';
import '../styles/globals.css';

function MyApp({ Component, pageProps }: AppProps) {
  return (
    <AuthProvider>
      <Component {...pageProps} />
    </AuthProvider>
  );
}

export default MyApp;
```

Пример защищенной страницы:

```typescript
// pages/dashboard.tsx
import withAuth from '../components/withAuth';
import useAuth from '../hooks/useAuth';

const Dashboard = () => {
  const { state, logout } = useAuth();

  return (
    <div className="p-6 max-w-4xl mx-auto">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Личный кабинет</h1>
        <button
          onClick={logout}
          className="bg-red-500 hover:bg-red-700 text-white font-bold py-2 px-4 rounded"
        >
          Выйти
        </button>
      </div>
      
      <div className="bg-white shadow-md rounded-lg p-6">
        <h2 className="text-2xl font-semibold mb-4">Профиль пользователя</h2>
        <div className="space-y-3">
          <p><strong>Имя:</strong> {state.user?.fullName}</p>
          <p><strong>Email:</strong> {state.user?.email}</p>
          <p><strong>Имя пользователя:</strong> {state.user?.username}</p>
          <p><strong>Роли:</strong> {state.user?.roles.join(', ')}</p>
        </div>
      </div>
    </div>
  );
};

export default withAuth(Dashboard);
```

## Заключение

Этот подход обеспечивает полноценную систему аутентификации с использованием JWT-токенов, обработку истечения их срока действия с помощью токенов обновления и защиту маршрутов для вашего приложения Next.js.

Основные особенности:

1. **Хранение токенов** в куках для безопасности.
2. **Автоматическое обновление токенов** при их истечении.
3. **Контекстный API** для глобального доступа к состоянию аутентификации.
4. **Защита маршрутов** на основе аутентификации и авторизации по ролям.
5. **Обработка ошибок** и отображение соответствующих сообщений пользователю.

Вы можете настроить этот подход в соответствии с конкретными требованиями вашего проекта.