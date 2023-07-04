namespace lang; 

public class CLine {
    public enum LineType {
        Rule,
        Function,
        Usual
    }
    
    public static void Unpack(Node node, Tree tree, LineType type) {
        if(node.source.name() == ";")
            return;
        
        if (type == LineType.Function && node.Hold as ReturnStmt != null) 
            CReturn.Unpack(node, tree);
        else if (node.Hold as Expr.AssignExpr != null)
            CAssign.Unpack(node, tree);
        else if (node.Hold as Expr.BinaryExpr != null)
            CBinaryExpr.Unpack(node, tree);
        else if (node.Hold as Expr.UnaryExpr != null)
            CUnaryExpr.Unpack(node, tree);
        else if (node.Hold as Expr.InvokeExpr != null)
            CInvokeExpr.Unpack(node, tree);
        else if (node.Hold as Call != null)
            CCall.Unpack(node, tree);
        else if (node.Hold as Function.FunctionStatement != null)
            CFunction.Unpack(node, tree);
        else if (node.Hold as Variable.VariableStatement != null)
            CVariable.Unpack(node, tree);
        else if (node.Hold as ForStmt != null)
            CFor.Unpack(node, tree, type);
        else if (node.Hold as WhileStmt != null)
            CWhile.Unpack(node, tree, type);
        else if (node.Hold as IfStmt != null)
            CIf.Unpack(node, tree, type);
        else if (node.Hold as Unoptimize != null)
            CUnoptimize.Unpack(node, tree, type);
        else 
            Compiler.Error("Invalid line.", node);
    }
}
