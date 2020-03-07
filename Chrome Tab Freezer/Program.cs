using System;
using System.Windows.Forms;
using Chrome_Tab_Freezer.ExitManagement;
using Chrome_Tab_Freezer.Types;
using NLog;

namespace Chrome_Tab_Freezer
{
	public static class Program
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger ( );

		private static ChromeMonitor _monitor;
		private static HookManager   _hookManager;

		public static void Main ( )
		{
			var settings = FreezerSettings.Instance;
			_monitor     = CreateMonitor ( settings );
			_hookManager = new HookManager ( settings, _monitor );
			_hookManager.Subscribe ( );

			AppExitHook.OnExit += OnExit;
			Application.Run ( );
		}

		private static void OnExit ( CtrlType type )
		{
			Logger.Info ( $"Encountered {type}: Exiting" );

			_hookManager.Unsubscribe ( );
			_monitor?.Stop ( );
			_monitor?.ResumeTabs ( );
		}

		private static ChromeMonitor CreateMonitor ( FreezerSettings settings )
		{
			var monitor = new ChromeMonitor ( settings );
			monitor.StateChanged.Subscribe ( OnStateChanged );
			monitor.Errors.Subscribe ( OnFreezeError );
			monitor.TabsStateChanged.Subscribe ( OnTabsStateChanged );
			return monitor;
		}

		private static void OnStateChanged ( ChromeMonitorState state )
		{
			Logger.Info ( $"Chrome monitor: {state}" );
		}

		private static void OnFreezeError ( FreezeError error )
		{
			Logger.Error ( error );
		}

		private static void OnTabsStateChanged ( ChromeTabsState state )
		{
			Logger.Info ( $"Tabs State changed: {state}" );
		}
	}
}