using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Chrome_Tab_Freezer.Extensions
{
	[PublicAPI]
	public static class ProcessExtension
	{
		public static void Suspend ( this Process process )
		{
			foreach ( ProcessThread thread in process.Threads )
			{
				var pOpenThread = OpenThread ( ThreadAccess.SUSPEND_RESUME, false, (uint) thread.Id );
				if ( pOpenThread == IntPtr.Zero )
					continue;

				SuspendThread ( pOpenThread );
				CloseHandle ( pOpenThread );
			}
		}

		public static void Resume ( this Process process )
		{
			foreach ( ProcessThread thread in process.Threads )
			{
				var pOpenThread = OpenThread ( ThreadAccess.SUSPEND_RESUME, false, (uint) thread.Id );
				if ( pOpenThread == IntPtr.Zero )
					continue;

				var suspendCount = int.MaxValue;
				while ( suspendCount > 1 )
					suspendCount = ResumeThread ( pOpenThread );
				CloseHandle ( pOpenThread );
			}
		}

#region WinApi

		[DllImport ( "kernel32.dll" )]
		[return: MarshalAs ( UnmanagedType.Bool )]
		private static extern bool CloseHandle ( IntPtr hObject );

		[DllImport ( "kernel32.dll" )]
		private static extern IntPtr OpenThread ( ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId );

		[DllImport ( "kernel32.dll" )]
		private static extern int SuspendThread ( IntPtr hThread );

		[DllImport ( "kernel32.dll" )]
		private static extern int ResumeThread ( IntPtr hThread );

#endregion
	}
}