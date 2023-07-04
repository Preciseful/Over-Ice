namespace lang; 

public class CArray : Tree, Leaf {
    public Tree? tree { get; set; }
    private List<Leaf> _leaves;
    
    private string _type;
    
    public List<Leaf> leaves() => _leaves;
    public string name() => "array";
    public string type() => _type;

    public CArray(string type, List<Leaf> leaves, Tree tree) {
        this._type = type;
        this._leaves = leaves;
        this.tree = tree;
        tree.leaves().Add(this);
    }

    public static CArray Unpack(Node nodes, Tree tree, bool compileConstant = false) {
        List<Leaf> leaves = new List<Leaf>();
        string fulltype = "array ", type = "";

        foreach (Node node in nodes.Children) {
            Leaf expr = CExpr.Convert(node, tree, compileConstant);
            tree.leaves().Remove(expr);
            leaves.Add(expr);
            if(leaves.Exists(l => l.type() == "array") && nodes.Children.Count > 1)
                Compiler.Error("Cannot contain additional arrays if there's an empty one.", node);
            
            if(type == "")
                type = expr.type();
            else if(type != expr.type())
                Compiler.Error("Unmatching array object type.", node);
        }

        fulltype += type;
        fulltype = fulltype.TrimEnd();
        return new CArray(fulltype, leaves, tree);
    }

    public static string GetType(Node type) {
        string typestr = "array ";
        while(type.Children.Count > 0) {
            type = type.Children[0];
                
            if(type.Hold as Identifier.ArrayType != null)
                typestr += "array ";
            else {
                typestr += type.source.name();
                break;
            }
        }

        return typestr;
    }

    public static Leaf UnpackIndexAccess(Node indexaccess, Tree tree) {
        Leaf lhv = CExpr.Convert(indexaccess.Get(0)!, tree),
            index = CExpr.Convert(indexaccess.Get(1)!, tree);

        tree.leaves().Remove(lhv);
        tree.leaves().Remove(index);
        
        if(!lhv.type().Contains("array"))
            Compiler.Error("Cannot index a non-array left-hand value.", indexaccess);
            
        // remove the first "array"
        string type = lhv.type().Remove(0, 5).Trim();
        return new CArrayIndex(type, lhv, index, tree);
    }
}