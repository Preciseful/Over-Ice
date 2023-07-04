namespace lang; 

public class CElse : Leaf, Tree {
    private List<Leaf> _leaves = new List<Leaf>();

    public List<Leaf> leaves() => _leaves;
    public Tree? tree {get; set;}
    public string name() => "else";
    public string type() => "void";

    public CElse(Tree tree) {
        this.tree = tree;
        tree.leaves().Add(this);
    }

    public static CElse Unpack(Node node, Tree tree, CLine.LineType type) {
        CElse cElse = new CElse(tree);
        Node? block = node.Consume<Block>();
        if (block == null)
            block = node.Consume(0)!;

        if (block.Hold as Block != null) {
            foreach (Node line in block.Children)
                CLine.Unpack(line, cElse, type);
        }
        else 
            CLine.Unpack(block, cElse, type);

        return cElse;
    }
}