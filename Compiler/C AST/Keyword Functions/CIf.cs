namespace lang; 

public class CIf : Tree, Leaf {
    private List<Leaf> _leaves = new List<Leaf>();
    public Leaf expr;
    public List<Leaf> Elifs = new List<Leaf>();
    public Leaf? Else;
    
    public List<Leaf> leaves() => _leaves;
    public Tree? tree {get; set;}
    public string name() => "if";
    public string type() => "void";

    public CIf(Leaf expr, Tree tree) {
        this.expr = expr;
        this.tree = tree;
        tree.leaves().Add(this);
    }
    
    public static CIf Unpack(Node node, Tree tree, CLine.LineType type) {
        CIf cIf = new CIf(CExpr.Convert(node.Consume(0)!, tree), tree);
        tree.leaves().Remove(cIf.expr);
        Node? block = node.Consume<Block>();
        if (block == null)
            block = node.Consume(0)!;

        if (block.Hold as Block != null) {
            foreach (Node line in block.Children)
                CLine.Unpack(line, cIf, type);
        }
        else 
            CLine.Unpack(block, cIf, type);

        Node? elif;
        while ((elif = node.Consume<ElifStmt>()) != null) {
            CElif celif = CElif.Unpack(elif, tree, type);
            tree.leaves().Remove(celif);
            cIf.Elifs.Add(celif);
        }
        
        Node? Else;
        if ((Else = node.Consume<ElseStmt>()) != null) {
            CElse cElse = CElse.Unpack(Else, tree, type);
            tree.leaves().Remove(cElse);
            cIf.Else = cElse;
        }

        return cIf;
    }
}