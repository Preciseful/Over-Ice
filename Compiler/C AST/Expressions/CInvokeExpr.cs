namespace lang; 

public class CInvokeExpr : Tree, Leaf {
    private List<Leaf> _leaves = new List<Leaf>();
    public Tree? tree { get; set; }
    private string _type;

    public List<Leaf> leaves() => _leaves;

    public Leaf LatestChild;

    public string name() => ".";
    public string type() => _type;
    
    public CInvokeExpr(Node expr, Tree tree) {
        this.tree = tree;
        tree.leaves().Add(this);
        Leaf lhv = CExpr.Convert(expr.Get(0)!, this), rhv = CExpr.Convert(expr.Get(1)!, this);
        if(lhv.type() == "void")
            Compiler.Error("Cannot invoke on a void value.", expr);
        
        VerifyInvokeIntegrity(expr, lhv, rhv);
        LatestChild = (rhv as CInvokeExpr)?.LatestChild ?? rhv;
        
        _type = LatestChild.type();
    }

    public static Leaf Unpack(Node expr, Tree tree) {
        CInvokeExpr cexpr = new CInvokeExpr(expr, tree);
        return cexpr;
    }
    
    public static Leaf VerifyEnumInvoke(CEnum lhv, Node value, Tree tree) {
        CEnum.Invoke invoke = new CEnum.Invoke(lhv.type(), value.source.name(), lhv, tree);
        tree.leaves().Add(invoke);
        if (lhv.values.Exists(v => v.name() == value.source.name()))
            return invoke;
        Compiler.Error("Unknown enum member.", value);
        return null;
    }

    public static void VerifyInvokeIntegrity(Node expr, Leaf lhv, Leaf rhv) {
        if (rhv as CInvokeExpr != null)
            rhv = ((CInvokeExpr)rhv).leaves()[0];

        switch (lhv.type()) {
            case "Player":
                if (rhv as CVariable != null && ((CVariable)rhv).accessibility.name != "local")
                    Compiler.Error("Global variables can't be called from a player.", expr.Get(1)!);
                break;
        }
    }
}