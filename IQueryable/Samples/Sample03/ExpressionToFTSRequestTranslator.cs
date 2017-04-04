using Sample03.E3SClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sample03
{
    public class ExpressionToFTSRequestTranslator : ExpressionVisitor
    {
        StringBuilder resultString;

        public string Translate(Expression exp)
        {
            resultString = new StringBuilder();
            Visit(exp);

            return resultString.ToString();
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable)
                && node.Method.Name == "Where")
            {
                var predicate = node.Arguments[1];
                Visit(predicate);

                return node;
            }

            if (node.Method.DeclaringType == typeof(string))
                return VisitStringMethod(node);

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                    return VisitEquals(node);

                case ExpressionType.AndAlso:
                    return VisitAnd(node);

                default:
                    throw new NotSupportedException(string.Format("Operation {0} is not supported", node.NodeType));
            };
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            resultString.Append(node.Member.Name).Append(":");

            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            resultString.Append(node.Value);

            return node;
        }

        private Expression VisitEquals(BinaryExpression node)
        {
            Expression memberNode;
            Expression constantNode;
            if (!((TestSetNode(node.Left, ExpressionType.MemberAccess, out memberNode) && TestSetNode(node.Right, ExpressionType.Constant, out constantNode)
                || (TestSetNode(node.Right, ExpressionType.MemberAccess, out memberNode) && TestSetNode(node.Left, ExpressionType.Constant, out constantNode)))))
                throw new NotSupportedException("Operands should be a constant and member access");

            Visit(memberNode);
            resultString.Append("(");
            Visit(constantNode);
            resultString.Append(")");

            return node;
        }

        private Expression VisitAnd(BinaryExpression node)
        {
            Visit(node.Left);
            resultString.Append(" AND ");
            Visit(node.Right);

            return node;
        }

        private Expression VisitStringMethod(MethodCallExpression node)
        {
            var member = node.Object as MemberExpression;
            if (member == null)
                return base.VisitMethodCall(node);

            var containingObject = member.Expression;
            if (!typeof(E3SEntity).IsAssignableFrom(containingObject.Type))
                return base.VisitMethodCall(node);

            if (node.Arguments.Count != 1)
                return base.VisitMethodCall(node);

            string constantPrefix = string.Empty;
            string constantPostfix = string.Empty;
            if (node.Method.Name == "Contains")
                constantPrefix = constantPostfix = "*";
            else if (node.Method.Name == "StartsWith")
                constantPostfix = "*";
            else if (node.Method.Name == "EndsWith")
                constantPrefix = "*";
            else
                return base.VisitMethodCall(node);

            Visit(node.Object);
            resultString.Append("(");
            resultString.Append(constantPrefix);
            Visit(node.Arguments[0]);
            resultString.Append(constantPostfix);
            resultString.Append(")");

            return node;
        }

        private static bool TestSetNode(Expression node, ExpressionType type, out Expression result)
        {
            if (node.NodeType == type)
            {
                result = node;
                return true;
            }

            result = null;
            return false;
        }
    }
}
