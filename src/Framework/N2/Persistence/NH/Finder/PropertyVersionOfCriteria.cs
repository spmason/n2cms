using System;
using System.Collections.Generic;
using System.Text;
using N2.Persistence.Finder;

namespace N2.Persistence.NH.Finder
{
	public class PropertyVersionOfCriteria : ICriteria<ContentItem>
	{
		Operator op;
		QueryBuilder query;

		public PropertyVersionOfCriteria(QueryBuilder query)
		{
			this.query = query;
			this.op = query.CurrentOperator;
		}

		#region IEqualityCriteria<ContentItem> Members

		public IQueryAction Eq(ContentItem value)
		{
			query.Versions = VersionOption.Include;
			query.Criterias.Add(new VersionOfHqlProvider(this.op, value));
			return query;//new QueryAction(this.query);
		}

		public IQueryAction NotEq(ContentItem value)
		{
			throw new NotImplementedException("Sorry, NotEq isn't supported.");
		}

		public IQueryAction In(params ContentItem[] anyOf)
		{
			throw new NotImplementedException("Sorry, In isn't supported.");
		}
		#endregion

	}
}
