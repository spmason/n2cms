using N2.Definitions;

namespace N2.Tests.Fakes
{
	public class FakeAttributeExplorer : AttributeExplorer
	{
		public FakeAttributeExplorer()
			: base(new FakeDependencyInjector())
		{
		}
	}
}