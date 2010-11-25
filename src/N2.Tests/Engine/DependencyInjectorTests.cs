using N2.Engine;
using NUnit.Framework;
using Rhino.Mocks;

namespace N2.Tests.Engine
{
	[TestFixture]
	public class DependencyInjectorTests
	{
		private interface IDependency{}
		private class Dependency : IDependency{}

		private class DependentService
		{
			[Dependency]
			protected internal IDependency ProtectedDependentProperty { get; set; }

			[Dependency]
			public IDependency PublicDependentProperty { get; set; }
		}

		private IServiceContainer container;

		[SetUp]
		public void SetUp()
		{
			container = MockRepository.GenerateStrictMock<IServiceContainer>();
			container.Expect(c => c.Resolve(typeof(IDependency)))
				.Repeat.Twice()
				.Return(new Dependency());
		}

		[TearDown]
		public void TearDown()
		{
			container.VerifyAllExpectations();
		}

		[Test]
		public void CanInjectPublicDependenciesIntoService()
		{
			var injector = new DependencyInjector(container);

			var service = new DependentService();
			injector.InjectDependents(service);

			Assert.That(service.PublicDependentProperty, Is.Not.Null);
			Assert.That(service.PublicDependentProperty, Is.InstanceOf<Dependency>());
		}

		[Test]
		public void CanInjectProtectedDependenciesIntoService()
		{
			var injector = new DependencyInjector(container);

			var service = new DependentService();
			injector.InjectDependents(service);

			Assert.That(service.ProtectedDependentProperty, Is.Not.Null);
			Assert.That(service.ProtectedDependentProperty, Is.InstanceOf<Dependency>());
		}
	}
}