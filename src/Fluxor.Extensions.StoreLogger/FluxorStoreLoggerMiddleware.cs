using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Fluxor.Extensions.StoreLogger
{
    public class FluxorStoreLoggerMiddleware : Middleware
    {
        private readonly IJSRuntime _jSRuntime;
        private object _prevAction;
        private dynamic _prevState;
        private DateTime _start;
        private IStore _store;

        public FluxorStoreLoggerMiddleware(IJSRuntime jSRuntime)
        {
            this._jSRuntime = jSRuntime;
        }

        public override async Task InitializeAsync(IStore store)
        {
            _store = store;
            await base.InitializeAsync(store).ConfigureAwait(false);
        }

        public override void BeforeDispatch(object action)
        {
            _start = DateTime.Now;
            _prevAction = action;
            _prevState = new ExpandoObject();
            foreach (var feature in _store.Features)
            {
                ((IDictionary<string, object>)_prevState).Add(feature.Key, feature.Value.GetState());
            }
        }

        public override void AfterDispatch(object action)
        {
            var _ = WriteToLog(action);
        }

        private async Task WriteToLog(object action)
        {
            var end = DateTime.Now;
            var duration = end - _start;
            var nextState = new ExpandoObject();
            foreach (var feature in _store.Features)
            {
                ((IDictionary<string, object>)nextState).Add(feature.Key, feature.Value.GetState());
            }
            string group = $"action @ {_start} {action.GetType().Name}";
            await _jSRuntime.InvokeVoidAsync("window.console.group", "%c" + group, "color:black;font-weight: bold;");

            await _jSRuntime.InvokeVoidAsync("window.console.log", "%c" + "prev state", "color:gray;");
            await _jSRuntime.InvokeVoidAsync("window.console.dir", (object)_prevState);
            await _jSRuntime.InvokeVoidAsync("window.console.log", "%c" + "action", "color:gray;");
            await _jSRuntime.InvokeVoidAsync("window.console.dir", _prevAction);

            await _jSRuntime.InvokeVoidAsync("window.console.log", "%c" + "next state", "color:gray;");
            await _jSRuntime.InvokeVoidAsync("window.console.dir", (object)nextState);

            await _jSRuntime.InvokeVoidAsync("window.console.groupEnd", group);
        }

    }
}
