using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Livet;
using SylphyHorn.Properties;
using SylphyHorn.Serialization;

namespace SylphyHorn.Services
{
	/// <summary>
	/// Provides access to multilingualized resources.
	/// </summary>
	public class ResourceService : NotificationObject
	{
		#region static members

		public static ResourceService Current { get; } = new ResourceService();

		#endregion
		
		private readonly string[] _supportedCultureNames =
		{
			"en",
			"ja",
			"zh-hans",
		};

		/// <summary>
		/// Gets multilingualized resource.
		/// </summary>
		public Resources Resources { get; }

		/// <summary>
		/// Gets a supported cultures.
		/// </summary>
		public IReadOnlyCollection<CultureInfo> SupportedCultures { get; }

		private ResourceService()
		{
			this.Resources = new Resources();
			this.SupportedCultures = this._supportedCultureNames
				.Select(x =>
				{
					try
					{
						return CultureInfo.GetCultureInfo(x);
					}
					catch (CultureNotFoundException)
					{
						return null;
					}
				})
				.Where(x => x != null)
				.ToList();
		}

		/// <summary>
		/// Change culture of resource using specified culture name.
		/// </summary>
		/// <param name="name">Culture name.</param>
		public void ChangeCulture(string name)
		{
			Resources.Culture = this.SupportedCultures.SingleOrDefault(x => x.Name == name);

			Settings.General.Culture.Value = Resources.Culture?.Name;
			this.RaisePropertyChanged(nameof(this.Resources));
		}
	}
}
