using Fluxor.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Fluxor.Extensions.StoreLogger
{
    public static class LocalStorageMiddlewareExtensions
    {
        public static Options AddStoreLoggerMiddleware(
            this Options options, StoreLoggerOptions loggerOptions)
        {
            options.Services.AddSingleton<StoreLoggerOptions>((prov) => loggerOptions);
            options = options.AddMiddleware<FluxorStoreLoggerMiddleware>();
            return options;
        }
    }
}
