namespace lang; 

public class CWhile : Tree, Leaf {
    private List<Leaf> _leaves = new List<Leaf>();
    public Leaf expr;

    public List<Leaf> leaves() => _leaves;
    public Tree? tree {get; set;}
    public string name() => "while";
    public string type() => "void";

    public CWhile(Leaf expr, Tree tree) {
        this.expr = expr;
        this.tree = tree;
        tree.leaves().Add(this);
    }
    
    public static CWhile Unpack(Node node, Tree tree, CLine.LineType type) {
        CWhile cWhile = new CWhile(CExpr.Convert(node.Consume(0)!, tree), tree);
        tree.leaves().Remove(cWhile.expr);
        cWhile.expr.tree = cWhile;
        
        Node? block = node.Consume<Block>();
        if (block == null)
            block = node.Consume(0)!;

        if (block.Hold as Block != null) {
            foreach (Node line in block.Children)
                CLine.Unpack(line, cWhile, type);
        }
        else 
            CLine.Unpack(block, cWhile, type);

        return cWhile;
    }
}
