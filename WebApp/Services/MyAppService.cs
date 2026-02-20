using Microsoft.Extensions.Options;
using WebApp.Settings;

namespace WebApp.Services
{
    public class MyAppService : IMyAppService
    {
        private readonly Microsoft.Extensions.Options.IOptions<MyAppSettings> options;

        public MyAppService(IOptions<MyAppSettings> options)
        {
            this.options = options;
        }
        public string GetMyApplicationName()
        {
            return this.options.Value.ApplicationName ?? string.Empty;
        }
    }
}
