namespace LibraryAPI.Services
{
    public interface ITemplateRenderer
    {
        Task<string> RenderAsync<TModel>(string templateName, TModel model);
    }
} 