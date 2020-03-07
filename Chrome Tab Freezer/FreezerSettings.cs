using System;
using System.Windows.Forms;
using ConfigManagement;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Chrome_Tab_Freezer
{
	[PublicAPI]
	public class FreezerSettings : PersistentClass<FreezerSettings>
	{
		public static readonly string FileName = "settings.json";

		public string   PidFilePath     { get; set; } = "pids.txt";
		public TimeSpan SuspendDuration { get; set; } = TimeSpan.FromSeconds ( 60 );
		public TimeSpan ResumeDuration  { get; set; } = TimeSpan.FromSeconds ( 2 );

		[JsonConverter ( typeof ( StringEnumConverter ) )]
		public Keys HotKey { get; set; } = Keys.Delete;

		public FreezerSettings ( ) : base ( FileName ) { }
	}
}