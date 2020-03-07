using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Chrome_Tab_Freezer.Types
{
	[PublicAPI]
	public class ChromeTabsState
	{
		public ImmutableList<int> Pids  { get; }
		public string             State { get; }

		public ChromeTabsState ( IEnumerable<int> pids, string state )
		{
			Pids  = pids.ToImmutableList ( );
			State = state;
		}

		public static ChromeTabsState Suspended ( IEnumerable<int> pids )
		{
			return new ChromeTabsState ( pids, "Suspended" );
		}

		public static ChromeTabsState Resumed ( IEnumerable<int> pids )
		{
			return new ChromeTabsState ( pids, "Resumed" );
		}

		public override string ToString ( )
		{
			return $"{State} {Pids.Count} tabs";
		}
	}
}