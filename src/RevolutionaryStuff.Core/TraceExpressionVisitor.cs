using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace RevolutionaryStuff.Core;

public class TraceExpressionVisitor : ExpressionVisitor
{
    private int VisitNumber;
    private readonly Stack<int> S = new Stack<int>();

    private Expression DoVisit<TE>(Func<TE, Expression> f, TE node, [CallerMemberName] string caller = null) where TE : Expression
    {
        Expression ret;
        S.Push(0);
        var indent = new string('\t', S.Count);
        Trace.WriteLine($"{indent}- {VisitNumber++}/{caller}/{node.NodeType}/{node}");
        ret = f(node);
        S.Pop();
        return ret;
    }

    protected override Expression VisitBinary(BinaryExpression node)
        => DoVisit(base.VisitBinary, node);

    protected override Expression VisitBlock(BlockExpression node)
        => DoVisit(base.VisitBlock, node);

    protected override CatchBlock VisitCatchBlock(CatchBlock node)
    {
        //            TraceVisit(node);
        return base.VisitCatchBlock(node);
    }

    protected override Expression VisitConditional(ConditionalExpression node)
        => DoVisit(base.VisitConditional, node);

    protected override Expression VisitConstant(ConstantExpression node)
        => DoVisit(base.VisitConstant, node);

    protected override Expression VisitDebugInfo(DebugInfoExpression node)
        => DoVisit(base.VisitDebugInfo, node);

    protected override Expression VisitDefault(DefaultExpression node)
        => DoVisit(base.VisitDefault, node);

    protected override Expression VisitDynamic(DynamicExpression node)
        => DoVisit(base.VisitDynamic, node);

    protected override ElementInit VisitElementInit(ElementInit node)
    {
        //            TraceVisit(node);
        return base.VisitElementInit(node);
    }

    protected override Expression VisitExtension(Expression node)
        => DoVisit(base.VisitExtension, node);

    protected override Expression VisitGoto(GotoExpression node)
        => DoVisit(base.VisitGoto, node);

    protected override Expression VisitIndex(IndexExpression node)
        => DoVisit(base.VisitIndex, node);

    protected override Expression VisitInvocation(InvocationExpression node)
        => DoVisit(base.VisitInvocation, node);

    protected override Expression VisitLabel(LabelExpression node)
        => DoVisit(base.VisitLabel, node);

    [return: NotNullIfNotNull("node")]
    protected override LabelTarget VisitLabelTarget(LabelTarget node)
    {
        //           TraceVisit(node);
        return base.VisitLabelTarget(node);
    }

    protected override Expression VisitLambda<T>(Expression<T> node)
        => DoVisit(base.VisitLambda, node);

    protected override Expression VisitListInit(ListInitExpression node)
        => DoVisit(base.VisitListInit, node);

    protected override Expression VisitLoop(LoopExpression node)
        => DoVisit(base.VisitLoop, node);

    protected override Expression VisitMember(MemberExpression node)
        => DoVisit(base.VisitMember, node);

    protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
    {
        //           TraceVisit(node);
        return base.VisitMemberAssignment(node);
    }

    protected override MemberBinding VisitMemberBinding(MemberBinding node)
    {
        //         TraceVisit(node);
        return base.VisitMemberBinding(node);
    }

    protected override Expression VisitMemberInit(MemberInitExpression node)
        => DoVisit(base.VisitMemberInit, node);

    protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
    {
        //       TraceVisit(node);
        return base.VisitMemberListBinding(node);
    }

    protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
    {
        //     TraceVisit(node);
        return base.VisitMemberMemberBinding(node);
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
        => DoVisit(base.VisitMethodCall, node);

    protected override Expression VisitNew(NewExpression node)
        => DoVisit(base.VisitNew, node);

    protected override Expression VisitNewArray(NewArrayExpression node)
        => DoVisit(base.VisitNewArray, node);

    protected override Expression VisitParameter(ParameterExpression node)
        => DoVisit(base.VisitParameter, node);

    protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        => DoVisit(base.VisitRuntimeVariables, node);

    protected override Expression VisitSwitch(SwitchExpression node)
        => DoVisit(base.VisitSwitch, node);

    protected override SwitchCase VisitSwitchCase(SwitchCase node)
    {
        //   TraceVisit(node);
        return base.VisitSwitchCase(node);
    }

    protected override Expression VisitTry(TryExpression node)
        => DoVisit(base.VisitTry, node);

    protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        => DoVisit(base.VisitTypeBinary, node);

    protected override Expression VisitUnary(UnaryExpression node)
        => DoVisit(base.VisitUnary, node);
}
