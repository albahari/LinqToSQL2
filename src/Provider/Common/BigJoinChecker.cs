using System.Data.Linq.Provider.NodeTypes;
using System.Data.Linq.Provider.Visitors;

namespace System.Data.Linq.Provider.Common
{
	internal class BigJoinChecker
	{
		#region Private Classes
		private class Visitor : SqlVisitor
		{
			internal bool canBigJoin = true;


			internal override SqlExpression VisitMultiset(SqlSubSelect sms)
			{
				return sms;
			}


			internal override SqlExpression VisitElement(SqlSubSelect elem)
			{
				return elem;
			}


			internal override SqlExpression VisitClientQuery(SqlClientQuery cq)
			{
				return cq;
			}


			internal override SqlExpression VisitExists(SqlSubSelect ss)
			{
				return ss;
			}


			internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss)
			{
				return ss;
			}


			internal override SqlSelect VisitSelect(SqlSelect select)
			{
				// big-joins may need to lift PK's out for default ordering, so don't allow big-join if we see these
				this.canBigJoin &= select.GroupBy.Count == 0 && select.Top == null && !select.IsDistinct;
				if(!this.canBigJoin)
				{
					return select;
				}
				return base.VisitSelect(select);
			}
		}

		#endregion

		internal static bool CanBigJoin(SqlSelect select)
		{
			Visitor v = new Visitor();
			v.Visit(select);
			return v.canBigJoin;
		}

		/// <summary>
		/// A big join reads 'count' consecutive rows per outer row, which is only correct when the final
		/// rowset is ordered such that each outer row's joined rows are contiguous. That ordering is
		/// created later (by OrderByLifter, from SqlSelect.OrderingType.Always) out of the identity
		/// members (primary keys) of the tables in the outer select's FROM clause. If any source lacks
		/// identity members - views, PK-less tables, table-valued functions - no such ordering can be
		/// produced, and the big join would silently associate rows with the wrong parents.
		/// </summary>
		internal static bool CanProvideDefaultOrdering(SqlNode node)
		{
			switch(node)
			{
				case null:
					// a FROM-less select produces a single row, so cannot compromise contiguity
					return true;
				case SqlTable table:
					return table.RowType.IdentityMembers.Count > 0;
				case SqlTableValuedFunctionCall tvf:
					return tvf.RowType.IdentityMembers.Count > 0;
				case SqlAlias alias:
					return CanProvideDefaultOrdering(alias.Node);
				case SqlJoin join:
					return CanProvideDefaultOrdering(join.Left) && CanProvideDefaultOrdering(join.Right);
				case SqlSelect select:
					// ordering cannot be lifted through GROUP BY or DISTINCT
					return select.GroupBy.Count == 0 && !select.IsDistinct && CanProvideDefaultOrdering(select.From);
				default:
					// unions, user queries (raw SQL), etc. provide no liftable ordering
					return false;
			}
		}
	}
}