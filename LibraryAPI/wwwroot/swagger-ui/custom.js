// Кастомный JavaScript для Swagger UI с JWT авторизацией

window.addEventListener('load', function() {
    console.log('Swagger UI JWT Enhancement loaded');
    
    // Функция для улучшения UI авторизации
    function enhanceAuthUI() {
        // Ищем кнопку авторизации
        const authorizeBtn = document.querySelector('.btn.authorize');
        if (authorizeBtn) {
            // Добавляем иконку замка
            if (!authorizeBtn.querySelector('.lock-icon')) {
                const lockIcon = document.createElement('span');
                lockIcon.className = 'lock-icon';
                lockIcon.innerHTML = '🔒 ';
                authorizeBtn.insertBefore(lockIcon, authorizeBtn.firstChild);
            }
            
            // Обновляем текст кнопки в зависимости от состояния авторизации
            const isAuthorized = authorizeBtn.classList.contains('unlocked');
            if (isAuthorized) {
                authorizeBtn.innerHTML = '🔓 Авторизован';
                authorizeBtn.style.backgroundColor = '#28a745';
            } else {
                authorizeBtn.innerHTML = '🔒 Авторизоваться';
                authorizeBtn.style.backgroundColor = '#dc3545';
            }
        }
    }
    
    // Функция для добавления подсказок к полям токена
    function enhanceTokenInput() {
        const observer = new MutationObserver(function(mutations) {
            mutations.forEach(function(mutation) {
                if (mutation.type === 'childList') {
                    // Ищем поле ввода токена
                    const tokenInput = document.querySelector('.auth-container input[type="text"]');
                    if (tokenInput && !tokenInput.hasAttribute('data-enhanced')) {
                        tokenInput.setAttribute('data-enhanced', 'true');
                        tokenInput.placeholder = 'Вставьте JWT токен здесь (без "Bearer")';
                        
                        // Добавляем валидацию токена
                        tokenInput.addEventListener('input', function() {
                            const token = this.value.trim();
                            const isValidJWT = isValidJWTFormat(token);
                            
                            if (token.length > 0) {
                                if (isValidJWT) {
                                    this.style.borderColor = '#28a745';
                                    this.style.backgroundColor = '#f8fff9';
                                } else {
                                    this.style.borderColor = '#dc3545';
                                    this.style.backgroundColor = '#fff8f8';
                                }
                            } else {
                                this.style.borderColor = '#ced4da';
                                this.style.backgroundColor = 'white';
                            }
                        });
                        
                        // Добавляем кнопку для очистки токена
                        const clearBtn = document.createElement('button');
                        clearBtn.type = 'button';
                        clearBtn.innerHTML = '🗑️ Очистить';
                        clearBtn.className = 'btn btn-sm btn-outline-secondary';
                        clearBtn.style.marginTop = '5px';
                        clearBtn.onclick = function() {
                            tokenInput.value = '';
                            tokenInput.style.borderColor = '#ced4da';
                            tokenInput.style.backgroundColor = 'white';
                        };
                        
                        if (!tokenInput.parentNode.querySelector('.clear-token-btn')) {
                            clearBtn.className += ' clear-token-btn';
                            tokenInput.parentNode.appendChild(clearBtn);
                        }
                    }
                }
            });
        });
        
        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    }
    
    // Функция для проверки формата JWT токена
    function isValidJWTFormat(token) {
        if (!token || typeof token !== 'string') return false;
        
        const parts = token.split('.');
        if (parts.length !== 3) return false;
        
        try {
            // Проверяем, что каждая часть является валидным base64
            parts.forEach(part => {
                if (part.length === 0) throw new Error('Empty part');
                // Добавляем padding если нужно
                const padded = part + '='.repeat((4 - part.length % 4) % 4);
                atob(padded.replace(/-/g, '+').replace(/_/g, '/'));
            });
            return true;
        } catch (e) {
            return false;
        }
    }
    
    // Функция для добавления информации о токене
    function addTokenInfo() {
        const observer = new MutationObserver(function(mutations) {
            mutations.forEach(function(mutation) {
                if (mutation.type === 'childList') {
                    const authContainer = document.querySelector('.auth-container');
                    if (authContainer && !authContainer.querySelector('.token-info')) {
                        const tokenInfo = document.createElement('div');
                        tokenInfo.className = 'token-info';
                        tokenInfo.innerHTML = `
                            <div style="background-color: #e7f3ff; border: 1px solid #b3d9ff; border-radius: 4px; padding: 10px; margin: 10px 0; font-size: 12px;">
                                <strong>💡 Как получить JWT токен:</strong><br>
                                1. Выполните POST запрос к <code>/auth/login</code><br>
                                2. Скопируйте значение <code>accessToken</code> из ответа<br>
                                3. Вставьте токен в поле выше (без префикса "Bearer")<br>
                                4. Нажмите "Authorize"<br><br>
                                <strong>🔍 Пример ответа login:</strong><br>
                                <code>{"accessToken": "eyJhbGci...", "refreshToken": "...", "user": {...}}</code>
                            </div>
                        `;
                        
                        const descriptionDiv = authContainer.querySelector('.auth-container-description');
                        if (descriptionDiv) {
                            descriptionDiv.parentNode.insertBefore(tokenInfo, descriptionDiv.nextSibling);
                        }
                    }
                }
            });
        });
        
        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    }
    
    // Функция для улучшения отображения ошибок авторизации
    function enhanceAuthErrors() {
        const observer = new MutationObserver(function(mutations) {
            mutations.forEach(function(mutation) {
                if (mutation.type === 'childList') {
                    // Ищем ошибки авторизации
                    const errorElements = document.querySelectorAll('.errors-wrapper');
                    errorElements.forEach(function(errorEl) {
                        if (!errorEl.hasAttribute('data-enhanced')) {
                            errorEl.setAttribute('data-enhanced', 'true');
                            errorEl.style.backgroundColor = '#f8d7da';
                            errorEl.style.border = '1px solid #f5c6cb';
                            errorEl.style.borderRadius = '4px';
                            errorEl.style.padding = '10px';
                            errorEl.style.marginTop = '10px';
                        }
                    });
                }
            });
        });
        
        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    }
    
    // Функция для добавления быстрых действий
    function addQuickActions() {
        // Добавляем кнопку для быстрого перехода к login
        const info = document.querySelector('.info');
        if (info && !info.querySelector('.quick-actions')) {
            const quickActions = document.createElement('div');
            quickActions.className = 'quick-actions';
            quickActions.innerHTML = `
                <div style="background-color: #d4edda; border: 1px solid #c3e6cb; border-radius: 4px; padding: 15px; margin: 20px 0;">
                    <h4 style="color: #155724; margin-top: 0;">🚀 Быстрый старт</h4>
                    <p style="margin-bottom: 10px;">Для тестирования API с авторизацией:</p>
                    <ol style="margin-bottom: 15px; padding-left: 20px;">
                        <li>Найдите метод <strong>POST /auth/login</strong> ниже</li>
                        <li>Нажмите "Try it out" и введите логин/пароль</li>
                        <li>Скопируйте <code>accessToken</code> из ответа</li>
                        <li>Нажмите кнопку <strong>🔒 Авторизоваться</strong> вверху страницы</li>
                        <li>Вставьте токен и нажмите "Authorize"</li>
                    </ol>
                    <button onclick="document.querySelector('[data-path=\\'/auth/login\\']').scrollIntoView({behavior: 'smooth'})" 
                            style="background-color: #007bff; color: white; border: none; padding: 8px 16px; border-radius: 4px; cursor: pointer;">
                        📍 Перейти к /auth/login
                    </button>
                </div>
            `;
            info.appendChild(quickActions);
        }
    }
    
    // Запускаем все улучшения
    setTimeout(function() {
        enhanceAuthUI();
        enhanceTokenInput();
        addTokenInfo();
        enhanceAuthErrors();
        addQuickActions();
        
        // Обновляем UI каждые 2 секунды
        setInterval(enhanceAuthUI, 2000);
    }, 1000);
    
    console.log('JWT Enhancement initialized');
}); 