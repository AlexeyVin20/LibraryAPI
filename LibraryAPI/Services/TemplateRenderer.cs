using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;

namespace LibraryAPI.Services
{
    public class TemplateRenderer : ITemplateRenderer
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public TemplateRenderer(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<string> RenderAsync<TModel>(string templatePath, TModel model)
        {
            var path = Path.Combine(_hostingEnvironment.ContentRootPath, templatePath);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Template not found at {path}");
            }

            var templateContent = await File.ReadAllTextAsync(path);

            if (model == null)
            {
                return templateContent;
            }

            if (model is IDictionary<string, object> dictionaryModel)
            {
                foreach (var kvp in dictionaryModel)
                {
                    var value = kvp.Value?.ToString() ?? "";
                    templateContent = templateContent.Replace($"{{{{{kvp.Key}}}}}", value);
                }
            }
            else
            {
                foreach (PropertyInfo prop in model.GetType().GetProperties())
                {
                    var value = prop.GetValue(model)?.ToString() ?? "";
                    templateContent = templateContent.Replace($"{{{{{prop.Name}}}}}", value);
                }
            }

            return templateContent;
        }
    }
} 