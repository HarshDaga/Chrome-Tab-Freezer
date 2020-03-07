using System;
using JetBrains.Annotations;

namespace Chrome_Tab_Freezer.Types
{
	[PublicAPI]
	public class FreezeError
	{
		public int       Pid       { get; internal set; }
		public string    Reason    { get; internal set; }
		public Exception Exception { get; internal set; }

		public FreezeError ( int pid, string reason = "", Exception exception = null )
		{
			Pid       = pid;
			Reason    = reason;
			Exception = exception;
		}

		public static FreezeError NotFound ( int pid )
		{
			return new FreezeError ( pid, "Process not found" );
		}

		public static FreezeError NotChrome ( int pid )
		{
			return new FreezeError ( pid, "Process is not chrome" );
		}

		public override string ToString ( )
		{
			return $"{Pid}: {Reason}";
		}
	}
}