using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Chrome_Tab_Freezer.Extensions;
using JetBrains.Annotations;

namespace Chrome_Tab_Freezer.Chrome
{
	[PublicAPI]
	public class ChromeTabsPidExtractor
	{
		public ImmutableDictionary<int, DateTime> BlackList { get; private set; } =
			ImmutableDictionary<int, DateTime>.Empty;

		public IList<int> GetPids ( )
		{
			var processes = Process.GetProcessesByName ( "chrome" );
			var args      = new Dictionary<int, ChromeCommandLineArgs> ( );
			foreach ( var process in processes )
			{
				if ( BlackList.ContainsKey ( process.Id ) )
					continue;

				var cmdLine = process.GetCommandLine ( );
				args[process.Id] = new ChromeCommandLineArgs ( cmdLine );
			}

			foreach ( var pid in args.Keys.Where ( x => !args[x].IsChromeTab ( ) ) )
				BlackList = BlackList.SetItem ( pid, DateTime.Now );

			return args.Keys.Except ( BlackList.Keys ).ToList ( );
		}

		public void ClearBlackList ( TimeSpan age )
		{
			var time = DateTime.Now - age;

			BlackList = BlackList
						.Where ( x => x.Value > time )
						.ToImmutableDictionary ( );
		}
	}
}