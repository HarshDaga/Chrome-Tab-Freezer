using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Chrome_Tab_Freezer.Chrome;
using Chrome_Tab_Freezer.Extensions;
using Chrome_Tab_Freezer.Types;
using JetBrains.Annotations;
using NLog;

namespace Chrome_Tab_Freezer
{
	[PublicAPI]
	public class ChromeMonitor
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger ( );

		public FreezerSettings    Settings  { get; private set; }
		public ImmutableList<int> Pids      { get; private set; }
		public ChromeTabsState    TabsState { get; private set; }
		public ChromeMonitorState State     { get; private set; }

		private readonly ChromeTabsPidExtractor  _extractor;
		private          CancellationTokenSource _cts;

		public ChromeMonitor ( FreezerSettings settings )
		{
			Settings  = settings;
			Pids      = ImmutableList<int>.Empty;
			TabsState = ChromeTabsState.Resumed ( Enumerable.Empty<int> ( ) );
			State     = ChromeMonitorState.Stopped;

			_extractor = new ChromeTabsPidExtractor ( );
		}

		public void UpdateSettings ( [NotNull] FreezerSettings settings )
		{
			Settings = settings;
		}

		public bool ReadPids ( )
		{
			var path = Settings.PidFilePath;
			if ( !File.Exists ( path ) )
			{
				Logger.Error ( $"File not found: {path}" );
				return false;
			}

			var lines = File.ReadAllLines ( path );
			var pids  = new List<int> ( );
			for ( var i = 0; i < lines.Length; i++ )
			{
				var line = lines[i];
				if ( !int.TryParse ( line, out var pid ) )
				{
					Logger.Warn ( $"{i}: {line} is not a valid PID" );
					continue;
				}

				pids.Add ( pid );
			}

			Pids = ImmutableList<int>.Empty.AddRange ( pids );

			return true;
		}

		public bool UpdatePids ( )
		{
			if ( !Settings.UseLivePids )
				return ReadPids ( );

			_extractor.ClearBlackList ( Settings.OldPidClearDuration );
			Pids = _extractor.GetPids ( ).ToImmutableList ( );
			return true;
		}

		public void SuspendTabs ( bool freeze = true )
		{
			var affectedPids = new List<int> ( );
			foreach ( var pid in Pids )
				try
				{
					if ( !TryGetProcess ( pid, out var process ) )
						continue;

					affectedPids.Add ( pid );
					if ( freeze )
						process.Suspend ( );
					else
						process.Resume ( );
				}
				catch ( Exception e )
				{
					_errorSubject.OnNext ( new FreezeError ( pid, exception: e ) );
				}


			SetTabsState ( freeze
							   ? ChromeTabsState.Suspended ( affectedPids )
							   : ChromeTabsState.Resumed ( affectedPids ) );
		}

		public void ResumeTabs ( )
		{
			SuspendTabs ( false );
		}

		public void Start ( )
		{
			if ( State == ChromeMonitorState.Running )
				return;

			Task.Run ( StartInternalAsync );
		}

		public void Stop ( )
		{
			_cts?.Cancel ( );
		}

		public void Toggle ( )
		{
			if ( State == ChromeMonitorState.Stopped )
				Start ( );
			else
				Stop ( );
		}

		private void SetState ( ChromeMonitorState state )
		{
			State = state;
			_stateChanged.OnNext ( state );
		}

		private void SetTabsState ( ChromeTabsState state )
		{
			TabsState = state;
			_tabsStateChanged.OnNext ( state );
		}

		private async Task StartInternalAsync ( )
		{
			SetState ( ChromeMonitorState.Running );

			_cts?.Cancel ( );
			_cts = new CancellationTokenSource ( );
			try
			{
				while ( !_cts.IsCancellationRequested )
				{
					if ( !UpdatePids ( ) )
						break;
					SuspendTabs ( );
					await Task
						  .Delay ( Settings.SuspendDuration, _cts.Token )
						  .ConfigureAwait ( false );
					_cts.Token.ThrowIfCancellationRequested ( );
					ResumeTabs ( );
					await Task
						  .Delay ( Settings.ResumeDuration, _cts.Token )
						  .ConfigureAwait ( false );
				}
			}
			catch ( TaskCanceledException ) { }
			catch ( OperationCanceledException ) { }

			ResumeTabs ( );
			SetState ( ChromeMonitorState.Stopped );
		}

		private bool TryGetProcess ( int pid, out Process process )
		{
			process = Process.GetProcessById ( pid );
			if ( process.HasExited )
			{
				_errorSubject.OnNext ( FreezeError.NotFound ( pid ) );
				return false;
			}

			if ( process.ProcessName != "chrome" )
			{
				_errorSubject.OnNext ( FreezeError.NotChrome ( pid ) );
				return false;
			}

			return true;
		}

#region Events

		public IObservable<FreezeError>        Errors           => _errorSubject.AsObservable ( );
		public IObservable<ChromeTabsState>    TabsStateChanged => _tabsStateChanged.AsObservable ( );
		public IObservable<ChromeMonitorState> StateChanged     => _stateChanged.AsObservable ( );

		private readonly Subject<FreezeError>        _errorSubject     = new Subject<FreezeError> ( );
		private readonly Subject<ChromeTabsState>    _tabsStateChanged = new Subject<ChromeTabsState> ( );
		private readonly Subject<ChromeMonitorState> _stateChanged     = new Subject<ChromeMonitorState> ( );

#endregion
	}
}