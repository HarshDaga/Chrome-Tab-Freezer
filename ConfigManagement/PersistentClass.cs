using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace ConfigManagement
{
	public abstract class PersistentClass<T> : INotifyPropertyChanged
		where T : PersistentClass<T>, INotifyPropertyChanged, new ( )
	{
		private static T _instance;

		public static T Instance
		{
			get => _instance;
			private set
			{
				if ( _instance != null )
					_instance.PropertyChanged -= InstanceOnPropertyChanged;

				_instance                 =  value;
				_instance.PropertyChanged += InstanceOnPropertyChanged;
			}
		}

		protected readonly string FilePath;

		static PersistentClass ( )
		{
			Instance = new T ( );
			Instance.Load ( );
			Instance.Save ( );
		}

		protected PersistentClass ( string filePath )
		{
			FilePath = filePath;
		}

#pragma warning disable CS0067
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

		private static void InstanceOnPropertyChanged ( object sender, PropertyChangedEventArgs e )
		{
			_instance.Save ( );
		}

		public void Save ( )
		{
			var json = JsonConvert.SerializeObject ( Instance, Formatting.Indented );
			File.WriteAllText ( FilePath, json );
		}

		// ReSharper disable once UnusedMethodReturnValue.Global
		public T Load ( )
		{
			if ( !File.Exists ( FilePath ) )
				return Instance;

			var json = File.ReadAllText ( FilePath );
			Instance = JsonConvert.DeserializeObject<T> ( json );

			return Instance;
		}

		[NotifyPropertyChangedInvocator]
		protected void OnPropertyChanged ( [CallerMemberName] string propertyName = null )
		{
			PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( propertyName ) );
		}
	}
}