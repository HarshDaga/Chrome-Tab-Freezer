using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Chrome_Tab_Freezer.ExitManagement
{
	[PublicAPI]
	public static class AppExitHook
	{
		static AppExitHook ( )
		{
			SetConsoleCtrlHandler ( ConsoleCtrlHandler, true );
		}

		[DllImport ( "Kernel32" )]
		private static extern bool SetConsoleCtrlHandler ( ExitEventHandler handler, bool add );

		public static event Action<CtrlType> OnExit;

		private static bool ConsoleCtrlHandler ( CtrlType type )
		{
			OnExit?.Invoke ( type );

			return true;
		}

		private delegate bool ExitEventHandler ( CtrlType type );
	}
}