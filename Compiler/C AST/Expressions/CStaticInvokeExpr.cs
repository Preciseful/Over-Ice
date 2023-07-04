namespace lang; 

public static class CStaticInvokeExpr {
    public static Leaf Unpack(Node expr, Tree tree) => Convert(expr, tree);
    
    public static Leaf Convert(Node expr, Tree tree) {
        Leaf lhv = CExpr.Convert(expr.Get(0)!, tree);
        tree.leaves().Remove(lhv);

        if (lhv as CEnum != null)
            return VerifyEnumInvoke((CEnum)lhv, expr.Get(1)!, tree);
        
        //todo classes & namespaces
        Compiler.Error("Invalid static invokement.", expr);
        return null;
    }
    
    public static Leaf VerifyEnumInvoke(CEnum lhv, Node value, Tree tree) {
        CEnum.Invoke invoke = new CEnum.Invoke(lhv.type(), value.source.name(), lhv, tree);
        tree.leaves().Add(invoke);
        if (lhv.values.Exists(v => v.name() == value.source.name()))
            return invoke;
        Compiler.Error("Unknown enum member.", value);
        return null;
    }
}