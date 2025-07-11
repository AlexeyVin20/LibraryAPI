using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;
using LibraryAPI.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SystemController : ControllerBase
    {
        private readonly ILogger<SystemController> _logger;

        public SystemController(ILogger<SystemController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Получить все доступные эндпоинты системы для ИИ-помощника
        /// </summary>
        [HttpGet("endpoints")]
        public ActionResult<object> GetAllEndpoints()
        {
            try
            {
                var endpoints = GetSystemEndpoints();

                return Ok(new
                {
                    Success = true,
                    Count = endpoints.Count,
                    Endpoints = endpoints,
                    GeneratedAt = DateTime.UtcNow,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка эндпоинтов");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера." });
            }
        }

        private List<EndpointInfo> GetSystemEndpoints()
        {
            var endpoints = new List<EndpointInfo>();
            var controllerTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(type => type.IsSubclassOf(typeof(ControllerBase)) && !type.IsAbstract);

            foreach (var controllerType in controllerTypes)
            {
                var controllerName = controllerType.Name.Replace("Controller", "");
                var routeAttribute = controllerType.GetCustomAttribute<RouteAttribute>();
                var baseRoute = routeAttribute?.Template?.Replace("[controller]", controllerName.ToLower()) ?? $"api/{controllerName.ToLower()}";

                var actions = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.IsPublic && !m.IsSpecialName && m.DeclaringType == controllerType &&
                                (m.GetCustomAttribute<HttpGetAttribute>() != null ||
                                 m.GetCustomAttribute<HttpPostAttribute>() != null ||
                                 m.GetCustomAttribute<HttpPutAttribute>() != null ||
                                 m.GetCustomAttribute<HttpDeleteAttribute>() != null ||
                                 m.GetCustomAttribute<HttpPatchAttribute>() != null));

                foreach (var action in actions)
                {
                    // Пропускаем сам эндпоинт, который отдает список
                    if (controllerType == typeof(SystemController) && action.Name == nameof(GetAllEndpoints))
                    {
                        continue;
                    }

                    var authorizeAttr = action.GetCustomAttribute<AuthorizeAttribute>() ??
                                      controllerType.GetCustomAttribute<AuthorizeAttribute>();
                    var allowAnonymousAttr = action.GetCustomAttribute<AllowAnonymousAttribute>();

                    var requiredRoles = new List<string>();
                    // Роли нужны только если эндпоинт не анонимный и есть атрибут авторизации с ролями
                    if (allowAnonymousAttr == null && authorizeAttr?.Roles != null)
                    {
                        requiredRoles = authorizeAttr.Roles.Split(',').Select(r => r.Trim()).ToList();
                    }

                    var httpMethods = new List<string>();
                    string routeTemplate = null;

                    if (action.GetCustomAttribute<HttpGetAttribute>() is { } httpGet) {
                        httpMethods.Add("GET");
                        routeTemplate = httpGet.Template;
                    }
                    if (action.GetCustomAttribute<HttpPostAttribute>() is { } httpPost) {
                        httpMethods.Add("POST");
                        routeTemplate = httpPost.Template;
                    }
                    if (action.GetCustomAttribute<HttpPutAttribute>() is { } httpPut) {
                        httpMethods.Add("PUT");
                        routeTemplate = httpPut.Template;
                    }
                    if (action.GetCustomAttribute<HttpDeleteAttribute>() is { } httpDelete) {
                        httpMethods.Add("DELETE");
                        routeTemplate = httpDelete.Template;
                    }
                    
                    routeTemplate ??= action.GetCustomAttribute<RouteAttribute>()?.Template;
                    
                    var fullRoute = $"{baseRoute}/{routeTemplate ?? ""}".TrimEnd('/');
                    
                    endpoints.Add(new EndpointInfo
                    {
                        Controller = controllerName,
                        Action = action.Name,
                        Route = fullRoute,
                        Methods = httpMethods,
                        RequiredRoles = requiredRoles,
                        IsAnonymous = allowAnonymousAttr != null,
                        Parameters = action.GetParameters().Select(p => new LibraryAPI.Models.ParameterInfo
                        {
                            Name = p.Name,
                            Type = p.ParameterType.Name,
                            IsOptional = p.HasDefaultValue
                        }).ToList()
                    });
                }
            }

            return endpoints.OrderBy(e => e.Controller).ThenBy(e => e.Route).ToList();
        }
    }
} 