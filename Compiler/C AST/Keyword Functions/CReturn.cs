namespace lang; 

public class CReturn : Leaf, Tree {
    private List<Leaf> _leaves = new List<Leaf>();
    
    public List<Leaf> leaves() => _leaves;
    public Leaf expr;
    public Tree? tree {get; set;}
    
    public string name() => "return";
    public string type() => "void";

    public CReturn(Leaf expr, Tree tree) {
        this.expr = expr;
        this.tree = tree;
        tree.leaves().Add(this);
    }

    public static CReturn Unpack(Node node, Tree tree) {
        CReturn ret = new CReturn(CExpr.Convert(node.Get(0)!, tree), tree);
        tree.leaves().Remove(ret.expr);
        ret.expr.tree = ret;

        return ret;
    }
}