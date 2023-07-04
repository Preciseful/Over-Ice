namespace lang; 

public class CAssign : Tree, Leaf {
    private List<Leaf> _leaves = new List<Leaf>();
    public Tree? tree { get; set; }

    public List<Leaf> leaves() => _leaves;
    private string _name;

    public string name() => _name;
    public string type() => "void";

    public CAssign(string op, Leaf lhv, Leaf rhv, Tree tree) {
        _leaves.Add(lhv);
        _leaves.Add(rhv);
        this._name = op;
        this.tree = tree;
        tree.leaves().Add(this);
    }

    public static CAssign Unpack(Node node, Tree tree) {
        Leaf? lhv = CExpr.Convert(node.Get(0)!, tree), 
            rhv = CExpr.Convert(node.Get(1)!, tree);

        if(lhv as CVariable == null 
           && (lhv as CInvokeExpr)?.LatestChild as CVariable == null 
           && lhv as CArrayIndex == null)
            Compiler.Error("Left-hand value must be an assignable value.", node.Get(0)!);
        
        if(lhv as CVariable != null && (lhv as CVariable)!.attributes.Exists(a => a.name == "*") ||
           (lhv as CInvokeExpr)?.LatestChild as CVariable != null 
           && ((lhv as CInvokeExpr)!.LatestChild as CVariable)!.attributes.Exists(a => a.name == "*"))
            Compiler.Error("Cannot assign to address-variables.", node.Get(0)!);
        tree.leaves().Remove(lhv);
        tree.leaves().Remove(rhv);

        CExpr.VerifyTypes(node, lhv, rhv, true);
        
        CAssign assign = new CAssign(node.source.name(), lhv, rhv, tree);
        lhv.tree = rhv.tree = assign;
        return assign;
    }
}