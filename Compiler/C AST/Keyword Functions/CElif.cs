namespace lang; 

public class CElif : Leaf, Tree {
    private List<Leaf> _leaves = new List<Leaf>();
    public Leaf expr;

    public List<Leaf> leaves() => _leaves;
    public Tree? tree {get; set;}
    public string name() => "else if";
    public string type() => "void";

    public CElif(Leaf expr, Tree tree) {
        this.expr = expr;
        this.tree = tree;
        tree.leaves().Add(this);
    }

    public static CElif Unpack(Node node, Tree tree, CLine.LineType type) {
        CElif cElif = new CElif(CExpr.Convert(node.Consume(0)!, tree), tree);
        tree.leaves().Remove(cElif.expr);
        Node? block = node.Consume<Block>();
        if (block == null)
            block = node.Consume(0)!;

        if (block.Hold as Block != null) {
            foreach (Node line in block.Children)
                CLine.Unpack(line, cElif, type);
        }
        else 
            CLine.Unpack(block, cElif, type);

        return cElif;
    }
}
