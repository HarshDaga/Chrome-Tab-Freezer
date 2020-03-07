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
		private static extern bool SetConsoleCtrlHandler ( EventHandler handler, bool add );

		public static event Action<CtrlType> OnExit;

		private static bool ConsoleCtrlHandler ( CtrlType type )
		{
			OnExit?.Invoke ( type );

			//shutdown right away so there are no lingering threads
			Environment.Exit ( -1 );

			return true;
		}

		private delegate bool EventHandler ( CtrlType type );
	}
}