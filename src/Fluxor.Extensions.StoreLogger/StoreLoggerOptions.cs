using System;
using System.Collections.Generic;

namespace Fluxor.Extensions.StoreLogger
{
    public record StoreLoggerOptions
    {
        public bool Enabled { get; init; }

        public IList<string> ExcludedStores { get; init; }
        public IList<string> IncludedStores { get; init; }

        public Func<IFeature, bool> Filter { get; init; }

        public StoreLoggerOptions()
        {
            Enabled = true;
        }

    }
}
