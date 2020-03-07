using System;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using NLog;

namespace Chrome_Tab_Freezer
{
#pragma warning disable CA1063 // Implement IDisposable Correctly
	public class HookManager : IDisposable
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger ( );

		public FreezerSettings Settings { get; }
		public ChromeMonitor   Monitor  { get; }

		private IKeyboardMouseEvents _globalHook;

		public HookManager ( FreezerSettings settings, ChromeMonitor monitor )
		{
			Settings = settings;
			Monitor  = monitor;
		}

		public void Dispose ( )
		{
			_globalHook?.Dispose ( );
		}

		public void Subscribe ( )
		{
			Logger.Info ( "Installing low level keyboard hooks." );

			_globalHook = Hook.GlobalEvents ( );

			_globalHook.KeyDown += OnKeyDown;

			Logger.Info ( "Installed low level keyboard hooks." );
		}

		public void Unsubscribe ( )
		{
			_globalHook.KeyDown -= OnKeyDown;

			Logger.Info ( "Uninstalled low level keyboard hooks." );
		}

		private void OnKeyDown ( object sender, KeyEventArgs args )
		{
			if ( args.KeyCode != Settings.HotKey )
				return;

			Logger.Info ( $"{args.KeyCode} intercepted: Toggling monitor state" );
			args.Handled = true;
			Monitor.Toggle ( );
		}
	}
}
#pragma warning restore CA1063 // Implement IDisposable Correctly