using Fluxor.DependencyInjection;

namespace Fluxor.Extensions.StoreLogger
{
    public static class LocalStorageMiddlewareExtensions
    {
        public static Options AddStoreLoggerMiddleware(
            this Options options, StoreLoggerOptions loggerOptions)
        {
            options = options.AddMiddleware<FluxorStoreLoggerMiddleware>();
            return options;
        }
    }
}
