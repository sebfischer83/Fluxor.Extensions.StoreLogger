using Fluxor.Extensions.StoreLogger.Demo.Store;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fluxor.Extensions.StoreLogger.Demo.Pages
{
    public partial class Counter
    {
        [Inject]
        private IState<CounterState> CounterState { get; set; }

        [Inject]
        public IDispatcher Dispatcher { get; set; }

        private void IncrementCount()
        {
            var action = new IncrementCounterAction();
            Dispatcher.Dispatch(action);
        }
    }
}
