using System;
using System.Collections.Generic;

namespace N2.Definitions
{
	public interface IAttributeExplorer
	{
		/// <summary>
		/// Finds attributes on a type.
		/// </summary>
		/// <typeparam name="T">The type of attribute to find.</typeparam>
		/// <param name="typeToExplore">The type to explore</param>
		/// <returns>A list of attributes defined on the class or it's properties.</returns>
		IList<T> Find<T>(Type typeToExplore) where T : IUniquelyNamed;

		/// <summary>
		/// Maps properties on the class and it's properties to a dictionary.
		/// </summary>
		/// <typeparam name="T">The type of attribute to find.</typeparam>
		/// <param name="typeToExplore">The type to explore.</param>
		/// <returns>A dictionary of atributes.</returns>
		IDictionary<string, T> Map<T>(Type typeToExplore) where T : IUniquelyNamed;
	}
}