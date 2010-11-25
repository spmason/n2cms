using N2.Engine;

namespace N2.Tests.Fakes
{
	public class FakeDependencyInjector : IDependencyInjector
	{
		public void InjectDependents(object instance)
		{
			//noop
		}
	}
}