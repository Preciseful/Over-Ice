namespace lang; 

public class CUnoptimize : Tree, Leaf {
    private List<Leaf> _leaves = new List<Leaf>();

    public List<Leaf> leaves() => _leaves;
    public Tree? tree {get; set;}
    public string name() => "unoptimize";
    public string type() => "void";

    public CUnoptimize(Tree tree) {
        this.tree = tree;
        tree.leaves().Add(this);
    }
    
    public static CUnoptimize? Unpack(Node nodes, Tree tree, CLine.LineType lineType) {
        if (nodes.Get(0)!.Children.Count == 0)
            return null;
        
        CUnoptimize unoptimize = new CUnoptimize(tree);
        foreach (var node in nodes.Get(0)!.Children) 
            CLine.Unpack(node, unoptimize, lineType);

        return unoptimize;
    }
}