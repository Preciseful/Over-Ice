namespace lang;

public class LambdaVar : Leaf {
    public string _type, _name;
    
    public Tree? tree { get; set; }
    public string type() => _type;
    public string name() => _name;

    public LambdaVar(string name, string type, Tree tree) {
        this._name = name;
        this._type = type;
        this.tree = tree;
        tree.leaves().Add(this);
    }

    public static LambdaVar Unpack(string name, string type, Tree tree) {
        return new LambdaVar(name, type, tree);
    }
}

public class CLambda : Tree, Leaf {
    private string _type = "lambda";
    private List<Leaf> _leaves = new List<Leaf>();
    
    public Tree? tree { get; set; }
    public string name() => "lambda";
    public string type() => _type;
    public List<Leaf> leaves() => _leaves;
    public Node? unresolvedExpr;

    public CLambda(Node unresolvedExpr, Tree tree) {
        this.unresolvedExpr = unresolvedExpr;
        this.tree = tree;
        tree.leaves().Add(this);
    }
    
    public CLambda(string type, Tree tree) {
        this._type = type;
        this.tree = tree;
        tree.leaves().Add(this);
    }

    public static CLambda Unpack(Node node, Tree tree) {
        if(tree as CCall == null)
            Compiler.Error("Unexpected lambda expression.", node);

        return new CLambda(node, tree);
    }

    private static readonly List<string> sortValues = new List<string>()
        { "int", "Vector" };

    public static CLambda Unpack(Node node, string type, string returnType, Tree tree) {
        Node ident = node.Get(0)!;
        CLambda lambda;
        switch (returnType) {
            case "%sort":
                lambda = new CLambda(type.Insert(0, "array "), tree);
                break;
            
            default:
                lambda = new CLambda(returnType, tree);
                break;
        }
        
        LambdaVar.Unpack(ident.source.name(), type, lambda);

        if (returnType == "void") {
            CLine.Unpack(node.Get(1)!, lambda, CLine.LineType.Usual);
        }
        else {
            Leaf leaf = CExpr.Convert(node.Get(1)!, lambda);
            switch (returnType) {
                case "%sort":
                    if(!sortValues.Contains(type))
                        Compiler.Error("Unmatching types.", node.Get(1)!);
                    break;
                
                case "%map":
                    lambda._type = leaf.type().Insert(0, "array ");
                    break;
                
                default:
                    if(leaf.type() != returnType)
                        Compiler.Error("Unmatching types.", node.Get(1)!);
                    break;
            }
        }

        return lambda;
    }

    public static bool VerifyTypes(CVariable act, CLambda lambda) {
        string[] actChildTypes = CExpr.GetChildTypes(act.type());
        if (actChildTypes.Length - 1 == lambda.unresolvedExpr!.Get(0)!.Children.Count)
            return true;
        return false;
    }
}