namespace lang; 

public class CFor : Tree, Leaf {
    private List<Leaf> _leaves = new List<Leaf>();

    public List<Leaf> expr1 = new List<Leaf>(); 
    public Leaf? expr2, expr3;

    public List<Leaf> leaves() => _leaves;
    public Tree? tree {get; set;}
    public string name() => "for";
    public string type() => "void";

    public CFor(Tree tree) {
        this.tree = tree;
        tree.leaves().Add(this);
    }
    
    public static CFor Unpack(Node node, Tree tree, CLine.LineType type) {
        Node nexpr1 = node.Consume(0)!, nexpr2 = node.Consume(0)!, nexpr3 = node.Consume(0)!;
        CFor cFor = new CFor(tree);

        if (nexpr1.source.name() != ";") {
            CLine.Unpack(nexpr1, cFor, type);
            cFor.expr1 = cFor.leaves();
            cFor._leaves = new List<Leaf>();
        }

        if (nexpr2.source.name() != ";") {
            cFor.expr2 = CExpr.Convert(nexpr2, cFor);
            cFor.leaves().Remove(cFor.expr2);
        }
        
        if (nexpr3.source.name() != ";") {
            cFor.expr3 = CExpr.Convert(nexpr3, cFor);
            cFor.leaves().Remove(cFor.expr3);
        }
        
        Node? block = node.Consume<Block>();
        if (block == null)
            block = node.Consume(0)!;

        if (block.Hold as Block != null) {
            foreach (Node line in block.Children)
                CLine.Unpack(line, cFor, type);
        }
        else 
            CLine.Unpack(block, cFor, type);

        return cFor;
    }
}
