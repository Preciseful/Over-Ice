namespace lang; 

public class CCondition : Tree, Leaf {
    private List<Leaf> _leaves = new List<Leaf>();
    
    public List<Leaf> leaves() => _leaves;
    public Tree? tree {get; set;}
    public string name() => "condition";
    public string type() => "condition";

    public CCondition(Node expr, Tree tree) {
        this.tree = tree;
        Leaf cexpr = CExpr.Convert(expr, this);
        if(cexpr.type() != "bool")
            Compiler.Error("Expression must be comparable to a boolean", expr);
    }
}
