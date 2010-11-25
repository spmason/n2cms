using System.Reflection;

namespace N2.Engine
{
	[Service(typeof(IDependencyInjector))]
	public class DependencyInjector : IDependencyInjector
	{
		private readonly IServiceContainer container;

		public DependencyInjector(IServiceContainer container)
		{
			this.container = container;
		}

		public void InjectDependents(object instance)
		{
			var type = instance.GetType();
			foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (property.IsDefined(typeof(DependencyAttribute), true))
				{
					var service = container.Resolve(property.PropertyType);
					property.SetValue(instance, service,
									  BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
									  null, null, null);
				}
			}
		}
	}
}