using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ConfigManagement
{
	public interface IConfig<out TConfig>
		where TConfig : IConfig<TConfig>
	{
		[JsonIgnore]
		string ConfigFileName { get; }

		bool TryValidate ( out IList<Exception> exceptions );

		TConfig RestoreDefaults ( );
	}
}