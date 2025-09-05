using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace System.Data.Linq.Provider.Visitors
{
	internal class Localizer : ExpressionVisitor {
		Dictionary<Expression, bool> locals;

		internal Localizer(Dictionary<Expression, bool> locals) {
			this.locals = locals;
		}

		internal Expression Localize(Expression expression) {
			return this.Visit(expression);
		}

		internal override Expression Visit(Expression exp) {
			if (exp == null) {
				return null;
			}

			// NEW: For supporting Span<T>/ReadOnlySpan<T>
			if (this.locals.ContainsKey(exp)) {
				// For byref-like nodes (Span<T>/ReadOnlySpan<T>), keep the node but still localize its children.
				// This preserves the op_Implicit/AsSpan shape while turning captured locals inside into constants.
				if (exp.Type.IsByRefLike) {
					return base.Visit(exp);
				}
				return MakeLocal(exp);
			}
			if (exp.NodeType == (ExpressionType)InternalExpressionType.Known) {
				return exp;
			}
			return base.Visit(exp);
        }

        private static Expression MakeLocal(Expression e) {

			// NEW: For supporting Span<T>/ReadOnlySpan<T>
			// Preserve byref-like types (e.g., Span<T>/ReadOnlySpan<T>) to avoid boxing issues
			// and to keep Span/ReadOnlySpan patterns (op_Implicit/AsSpan) visible for downstream translators.
			if (e.Type.IsByRefLike) {
                return e;
            }

            if (e.NodeType == ExpressionType.Constant) {
                return e;
            }
            if (e.NodeType == ExpressionType.Convert || e.NodeType == ExpressionType.ConvertChecked) {
                UnaryExpression ue = (UnaryExpression)e;

				// NEW: For supporting Span<T>/ReadOnlySpan<T>
				// If the target type is byref-like, do not funcletize.
				if (ue.Type.IsByRefLike) {
                    return e;
                }

				if (ue.Type == typeof(object)) {
					Expression local = MakeLocal(ue.Operand);
					return (e.NodeType == ExpressionType.Convert) ? Expression.Convert(local, e.Type) : Expression.ConvertChecked(local, e.Type);
				}
				// convert a const null
				if (ue.Operand.NodeType == ExpressionType.Constant) {
					ConstantExpression c = (ConstantExpression)ue.Operand;
					if (c.Value == null) {
						return Expression.Constant(null, ue.Type);
					}
				}
			}
			return Expression.Invoke(Expression.Constant(Expression.Lambda(e).Compile()));
		}
	}
}