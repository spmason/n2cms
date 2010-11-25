namespace N2.Engine
{
	public interface IDependencyInjector
	{
		void InjectDependents(object instance);
	}
}