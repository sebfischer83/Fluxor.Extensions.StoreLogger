using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fluxor.Extensions.StoreLogger.Demo.Store
{
	public class CounterState
	{
		public int ClickCount { get; }

		public CounterState(int clickCount)
		{
			ClickCount = clickCount;
		}
	}

	public class Feature : Feature<CounterState>
	{
		public override string GetName() => "Counter";
		protected override CounterState GetInitialState() =>
			new CounterState(clickCount: 0);
	}

	public static class Reducers
	{
		[ReducerMethod]
		public static CounterState ReduceIncrementCounterAction(CounterState state, IncrementCounterAction action) =>
			new CounterState(clickCount: state.ClickCount + 1);
	}
}
