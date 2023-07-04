namespace lang; 

public class CBinaryExpr : Tree, Leaf {
    private List<Leaf> _leaves = new List<Leaf>();
    private string _type, _name;
    
    
    
    public List<Leaf> leaves() => _leaves;
    public COperator Operator;
    public Tree? tree {get; set;}
    
    public string name() => _name;
    public string type() => _type;
    
    public CBinaryExpr(Node expr, Tree tree, bool compileConstant = false) {
        this.tree = tree;
        tree.leaves().Add(this);
        Leaf? lhv, rhv;
        _name = expr.source.name();
        lhv = CExpr.Convert(expr.Get(0)!, this, compileConstant);
        rhv = CExpr.Convert(expr.Get(1)!, this, compileConstant);

        Operator = COperator.Unpack(_name, lhv.type());

        if(!Operator.VerifyTypes(lhv.type(), rhv.type()))
            Compiler.Error("Invalid operation.", expr);
        
        _type = Operator.GetOperandReturn(lhv.type(), rhv.type());
    }

    public static Leaf Unpack(Node expr, Tree tree) {
        CBinaryExpr cexpr = new CBinaryExpr(expr, tree);
        return cexpr;
    }
}
