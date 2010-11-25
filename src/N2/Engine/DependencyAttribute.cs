using System;

namespace N2.Engine
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class DependencyAttribute : Attribute
	{
	}
}