using LibraryAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using LibraryAPI.Services;
using LibraryAPI.Models;
using LibraryAPI.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Регистрация сервисов аутентификации
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Регистрация сервисов уведомлений
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHostedService<NotificationBackgroundService>();

// Добавление SignalR для push уведомлений
builder.Services.AddSignalR();

// Настройка аутентификации
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddAuthentication(options => 
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RequireExpirationTime = true,
        RequireSignedTokens = true
    };
    
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    
    // Добавляем обработку событий для отладки
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
            Console.WriteLine($"Request scheme: {context.Request.Scheme}, Host: {context.Request.Host}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"JWT Token validated successfully for scheme: {context.Request.Scheme}");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            Console.WriteLine($"JWT Message received for scheme: {context.Request.Scheme}, Path: {context.Request.Path}");
            
            // Для SignalR: токен может приходить в query string как access_token
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
            {
                context.Token = accessToken;
                Console.WriteLine("Using access_token from query string for SignalR");
            }
            else
            {
                // Обычный способ — из заголовка
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    context.Token = authHeader.Substring(7);
                    Console.WriteLine("Using Bearer token from Authorization header");
                }
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"JWT Challenge: {context.Error} - {context.ErrorDescription}");
            Console.WriteLine($"Request scheme: {context.Request.Scheme}, Host: {context.Request.Host}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.Formatting = Formatting.Indented;
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Настройка Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Библиотечная система API",
        Version = "3.0.0",
        Description = "API для управления библиотечной системой с JWT аутентификацией",
        Contact = new OpenApiContact
        {
            Name = "Поддержка",
            Email = "support@example.com"
        }
    });

    options.CustomSchemaIds(type => type.FullName);
    
    // Включаем поддержку аннотаций Swagger
    options.EnableAnnotations();

    // Добавление поддержки JWT-токенов в Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT токен авторизации. 
                      
                      Для получения токена:
                      1. Выполните POST запрос к /auth/login с логином и паролем
                      2. Скопируйте значение 'accessToken' из ответа
                      3. Введите только токен (без 'Bearer') в поле ниже
                      4. Нажмите 'Authorize'
                      
                      Пример токена: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "bearer",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });

    // Включение операций с JWT-токенами в Swagger
    options.OperationFilter<SecurityRequirementsOperationFilter>();
    
    // Настройка генерации XML-документации
    try
    {
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    }
    catch
    {
        // Игнорируем ошибки при загрузке XML-файла
    }
});

// Добавление поддержки Newtonsoft JSON для Swagger
builder.Services.AddSwaggerGenNewtonsoftSupport();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policyBuilder => policyBuilder
            // Указываем конкретные разрешенные источники. 
            // Добавьте сюда URL вашего фронтенда, если он отличается.
            .WithOrigins("https://localhost:3000", "http://localhost:3000", 
                        "https://localhost:3002", "http://localhost:3002",
                        "https://172.18.0.1:3002", "http://172.18.0.1:3002",
                        "http://localhost:5001", "https://localhost:5000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()); // Обязательно для SignalR с аутентификацией
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
    });
    
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Библиотечная система API v1");
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.EnableDeepLinking();
        c.DisplayRequestDuration();
        c.EnablePersistAuthorization(); // Сохраняет токен между сессиями
        c.EnableTryItOutByDefault(); // Включает "Try it out" по умолчанию
        c.SupportedSubmitMethods(Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Get, 
                               Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Post,
                               Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Put,
                               Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Delete);
        
        // Настройка темы и отображения
        c.DefaultModelExpandDepth(2);
        c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
        c.DisplayOperationId();
        c.DisplayRequestDuration();
    });
}

app.UseCors("AllowAll");

// Условное перенаправление на HTTPS только для production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Добавляем поддержку статических файлов для кастомных CSS/JS
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Добавление маршрутов SignalR
app.MapHub<NotificationHub>("/notificationHub");

app.MapControllers();

app.Urls.Add("http://*:5001");
app.Urls.Add("https://*:5000");

app.Run();
