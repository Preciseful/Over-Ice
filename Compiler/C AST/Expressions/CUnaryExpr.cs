namespace lang; 

public class CUnaryExpr : Tree, Leaf {
    private List<Leaf> _leaves = new List<Leaf>();
    private string _type, _name;
    public List<Leaf> leaves() => _leaves;

    public Tree? tree {get; set;}

    public string name() => _name;
    public string type() => _type;
    
    public CUnaryExpr(Node expr, Tree tree, bool compileConstant = false) {
        this.tree = tree;
        tree.leaves().Add(this);
        Leaf? value;
        _name = expr.source.name();
        
        value = CExpr.Convert(expr.Get(0)!, this, compileConstant);

        COperator cOperator = COperator.Unpack(_name, value.type(), this);
        CExpr.VerifyTypes(expr, value, cOperator, false);
        _type = cOperator.type();
    }

    public static Leaf Unpack(Node expr, Tree tree) {
        CUnaryExpr cexpr = new CUnaryExpr(expr, tree);
        return cexpr;
    }
}