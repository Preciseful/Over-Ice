namespace lang; 

public class CFunction : Tree, Leaf {
    private List<Leaf> _leaves = new List<Leaf>();
    
    private string _name, _type;
    
    public Node? block;
    public bool DirectReturn;
    public List<CVariable> parameters = new List<CVariable>();
    public List<CAttribute> attributes = new List<CAttribute>();
    public List<Leaf> leaves() => _leaves;
    public Tree? tree {get; set;}
    public string name() => _name;
    public string type() => _type;

    public CFunction(string name, string type, List<CVariable> parameters, List<CAttribute> attributes, Node block, Tree? tree = null) {
        this._name = name;
        this._type = type;
        this.tree = tree;
        this.parameters = parameters;
        this.attributes = attributes;
        this.block = block;
        if (block.Hold as Function.DirectReturn != null)
            DirectReturn = true;
        this.tree?.leaves().Add(this);
    }
    
    public CFunction(string name, string type, Tree? tree = null) {
        this._name = name;
        this._type = type;
        this.tree = tree;
        this.tree?.leaves().Add(this);
    }

    public static CFunction Unpack(Node fnode, Tree? tree = null) {
        Node? attributes = fnode.Consume<Attribute.Attributes>();
        Node identifier = fnode.Consume<Identifier>()!, type = fnode.Consume<Identifier, Identifier.Type, Identifier.ArrayType>()!, parameters = fnode.Consume<Function.Parameters>()!;
        List<CVariable> cparameters = new List<CVariable>();
        if(type.Hold as Identifier != null && !CVariable.CheckType(type, tree!))
            Compiler.Error("Unknown identifier.", type);
        
        string typestr = type.source.name();
        bool optionalparams = false;

        foreach (Node parameter in parameters.Children) {
            CVariable var = CVariable.Unpack(parameter, new CProgram())[0];
            cparameters.Add(var);
            
            if (var.initialvalue != null)
                optionalparams = true;
            else if(optionalparams)
                Compiler.Error("Optional parameters must come after non-optional ones.", parameter);
        }

        if(tree?.Find(f => f.name() == identifier.source.name()
                           && f as CFunction != null 
                           && CheckParams((CFunction)f, cparameters)) != null)
            Compiler.Error("Already defined function.", identifier);

        if (type.Hold as Identifier.ArrayType != null) 
            typestr = CArray.GetType(type);
        Node? block = fnode.Consume<Block>();
        if (block == null)
            block = fnode.Consume<Function.DirectReturn>();

        return new CFunction(identifier.source.name(), typestr, cparameters, CAttribute.Unpack(attributes), block!, tree);
    }

    private static bool CheckParams(CFunction opfunc, List<CVariable> param2) {
        List<CVariable> param1 = opfunc.parameters;
        if (param1.Count != param2.Count)
            return false;
        for (int i = 0; i < param1.Count; i++) {
            if (param1[i].type() != param2[i].type())
                return false;
        }

        return true;
    }
}