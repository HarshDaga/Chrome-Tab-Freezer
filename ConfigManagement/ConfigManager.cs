using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using Newtonsoft.Json;

// ReSharper disable StaticMemberInGenericType

namespace ConfigManagement
{
	public static class ConfigManager
	{
		private static readonly Subject<Exception> ExceptionObservableBf = new Subject<Exception> ( );

		[UsedImplicitly] public static IObservable<Exception> ExceptionObservable => ExceptionObservableBf;

		public static JsonSerializerSettings SerializerSettings { get; set; } = new JsonSerializerSettings
		{
			Formatting             = Formatting.Indented,
			NullValueHandling      = NullValueHandling.Ignore,
			ObjectCreationHandling = ObjectCreationHandling.Replace
		};

		public static ImmutableHashSet<Type> InitializedConfigs { get; private set; } = ImmutableHashSet<Type>.Empty;

		public static void SetInitialized<TConfig> ( )
			where TConfig : IConfig<TConfig>
		{
			InitializedConfigs = InitializedConfigs.Add ( typeof ( TConfig ) );
		}

		internal static void Throw ( Exception e )
		{
			ExceptionObservableBf.OnNext ( e );
		}
	}

	public static class ConfigManager<TConfig>
		where TConfig : IConfig<TConfig>, new ( )
	{
		private static readonly object FileLock = new object ( );

		private static TConfig _instance = new TConfig ( );

		[UsedImplicitly] public static ref TConfig Instance => ref _instance;

		public static Exception LastError { [UsedImplicitly] get; private set; }

		public static string FolderName => "Configs";

		public static string FileName =>
			Path.Combine ( FolderName, $"{_instance.ConfigFileName}.json" );

		public static string Serialized =>
			JsonConvert.SerializeObject ( _instance, ConfigManager.SerializerSettings );

		static ConfigManager ( )
		{
			Load ( );
			Save ( );

			ConfigManager.SetInitialized<TConfig> ( );
		}

		[UsedImplicitly]
		public static void ClearLastError ( )
		{
			LastError = null;
		}

		[UsedImplicitly]
		public static bool TryValidate ( out IList<Exception> exceptions )
		{
			return _instance.TryValidate ( out exceptions );
		}

		public static void Save ( )
		{
			try
			{
				lock ( FileLock )
				{
					if ( !Directory.Exists ( FolderName ) )
						Directory.CreateDirectory ( FolderName );
					File.WriteAllText ( FileName, Serialized );
				}
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch ( Exception e )
			{
				LastError = e;
				ConfigManager.Throw ( e );
			}
#pragma warning restore CA1031 // Do not catch general exception types
		}

		public static void Load ( )
		{
			try
			{
				lock ( FileLock )
				{
					if ( File.Exists ( FileName ) )
						_instance = JsonConvert.DeserializeObject<TConfig> ( File.ReadAllText ( FileName ),
																			 ConfigManager.SerializerSettings );
				}
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch ( Exception e )
			{
				LastError = e;
				ConfigManager.Throw ( e );
			}
#pragma warning restore CA1031 // Do not catch general exception types
		}

		[UsedImplicitly]
		public static void RestoreDefaults ( )
		{
			try
			{
				_instance = _instance.RestoreDefaults ( );
				lock ( FileLock )
				{
					if ( !Directory.Exists ( FolderName ) )
						Directory.CreateDirectory ( FolderName );
					File.WriteAllText ( FileName, Serialized );
				}
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch ( Exception e )
			{
				LastError = e;
				ConfigManager.Throw ( e );
			}
#pragma warning restore CA1031 // Do not catch general exception types
		}

		[UsedImplicitly]
		public static void Reset ( )
		{
			try
			{
				lock ( FileLock )
				{
					if ( File.Exists ( FileName ) )
						File.Delete ( FileName );

					_instance = new TConfig ( );

					if ( !Directory.Exists ( FolderName ) )
						Directory.CreateDirectory ( FolderName );
					File.WriteAllText ( FileName, Serialized );
				}
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch ( Exception e )
			{
				LastError = e;
				ConfigManager.Throw ( e );
			}
#pragma warning restore CA1031 // Do not catch general exception types
		}
	}
}