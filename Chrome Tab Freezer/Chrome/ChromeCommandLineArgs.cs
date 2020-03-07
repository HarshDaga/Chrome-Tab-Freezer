using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Chrome_Tab_Freezer.Chrome
{
	[PublicAPI]
	public class ChromeCommandLineArgs
	{
		public static readonly Regex OptionsRegex = new Regex ( @"--(?'key'[\w-]+)=(?'value'[\w-,]+)" );
		public static readonly Regex FlagsRegex   = new Regex ( @"--(?'flag'[\w-]*)\s" );

		public ImmutableDictionary<string, string> Options { get; private set; } =
			ImmutableDictionary<string, string>.Empty;

		public ImmutableList<string> Flags { get; private set; } = ImmutableList<string>.Empty;

		public ChromeCommandLineArgs ( string args )
		{
			if ( args is null )
				return;
			var index = args.IndexOf ( " --", StringComparison.Ordinal );
			if ( index == -1 )
				return;

			var sanitized = args.Substring ( index );
			ExtractOptions ( sanitized );
			ExtractFlags ( sanitized );
		}

		public bool IsChromeTab ( )
		{
			if ( Flags.Contains ( "extension-process" ) )
				return false;
			if ( Options.TryGetValue ( "type", out var type ) && type != "renderer" )
				return false;
			if ( !Flags.Contains ( "enable-auto-reload" ) )
				return false;

			return true;
		}

		private void ExtractOptions ( string args )
		{
			var matches = OptionsRegex.Matches ( args );
			foreach ( Match match in matches )
				Options = Options.SetItem ( match.Groups["key"].Value, match.Groups["value"].Value );
		}

		private void ExtractFlags ( string args )
		{
			var matches = FlagsRegex.Matches ( args );
			foreach ( Match match in matches )
				Flags = Flags.Add ( match.Groups["flag"].Value );
		}
	}
}