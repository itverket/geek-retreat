namespace Web.ViewModels
{
    public class SearchCriteria
    {
        public string Query { get; set; }
        public string Username { get; set; }
    }

    public class SearchCriteria2
    {
        public string Query { get; set; }
        public LogicalExpression Expression { get; set; }

        public string GetFilter()
        {
            return Expression.ToString();
        }
    }

    public interface IExpression
    {
    }

    public class SingleExpression : IExpression
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public ComparisonOperator Operator { get; set; }

        public override string ToString()
        {
            return $"{Name} {Operator.ToString().ToLower()} {Value}";
        }
    }

    public class ExpressionGroup : IExpression
    {
        public LogicalExpression Expression { get; set; }

        public override string ToString()
        {
            return $"({Expression})";
        }
    }

    public class LogicalExpression : IExpression
    {
        public IExpression Left { get; set; }
        public IExpression Right { get; set; }
        public LogicalOperator LogicalOperator { get; set; }

        public override string ToString()
        {
            if (Left == null && Right == null)
                return string.Empty;
            if (Right == null)
                return Left.ToString();
            if (Left == null)
                return Right.ToString();

            return $"{Left} {LogicalOperator.ToString().ToLower()} {Right}";
        }
    }

    public enum LogicalOperator
    {
        And,
        Or
    }

    public enum ComparisonOperator
    {
        Eq, Ne, Gt, Lt, Ge, Le
    }
}
