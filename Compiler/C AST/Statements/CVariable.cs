namespace lang; 

public class CVariable : Leaf {
    private string _name, _type;

    public Leaf? initialvalue;
    public List<CAttribute> attributes;
    public CAttribute accessibility;
    public Tree? tree {get; set;}
    
    // use initTree to later check in CWorkshop if it was declared at body-level
    public Tree initTree;
    
    public string name() => _name;
    public string type() => _type;
    
    public CVariable(string name, string type, List<CAttribute> attributes, Leaf? initialvalue, Tree tree) {
        this._name = name;
        this._type = type;
        this.initTree = tree;
        this.initialvalue = initialvalue;
        this.tree = tree;
        this.tree.leaves().Add(this);
        this.attributes = attributes;
        CAttribute? tempaccess = attributes.Find(a => a.name == "global" || a.name == "local");
        if (tempaccess != null) 
            attributes.Remove(tempaccess);
        else if (tree as CBody != null)
            tempaccess = new CAttribute("global");
        else
            tempaccess = new CAttribute("local");

        accessibility = tempaccess;
    }
    
    public static CVariable[] Unpack(Node vnode, Tree tree) {
        List<CVariable> variables = new List<CVariable>();
        List<CAttribute> attributes = CAttribute.Unpack(vnode.Consume<Attribute.Attributes>());
            
        while (vnode.Get<Identifier>() != null) {
            Node ident = vnode.Consume<Identifier>()!, type = vnode.Consume<Identifier, Identifier.Type, Identifier.ArrayType>()!;
            if(tree.Find(l => l as CVariable != null && l.name() == ident.source.name()) != null)
                Compiler.Error("Already defined variable.", ident);
            string typestr = type.source.name();
            if (type.Hold as Identifier.ArrayType != null) 
                typestr = CArray.GetType(type);
            if ((type.Hold as Identifier != null || type.Hold as Identifier.ArrayType != null) &&
                !CheckType(type, tree, typestr)) 
            {
                // for error, go down to the actual type, not the array definition
                while (type.Hold as Identifier.ArrayType != null)
                    type = type.Children[0];
                if(Compiler.Enums.ContainsKey(type.source.name()))
                    Compiler.Error("Invalid enum type.", type);
                if (type.Hold as Identifier.Type == null)
                    Compiler.Error("Unknown identifier.", type);
            }

            CVariable variable = new CVariable(ident.source.name(), typestr, attributes, null, tree);
            if (vnode.Get(0) != null && vnode.Get(0)!.Hold as Identifier == null) {
                Leaf InitVal = CExpr.Convert(vnode.Get(0)!, tree);
                CExpr.VerifyTypes(vnode.Get(0)!, variable, InitVal, true);
                variable.initialvalue = InitVal;
                vnode.Consume(0);
                tree.leaves().Remove(InitVal);
            }

            variables.Add(variable);
        }

        return variables.ToArray();
    }

    public static bool CheckType(Node type, Tree tree, string? typestr = null) {
        if (type.Hold as Identifier != null)
            return tree.Find(l => l.name() == type.source.name() &&
                                  l as CEnum != null) != null ||
                   Compiler.Enums[type.source.name()]?["type"] != null;

        if (typestr == null)
            return false;

        string name = typestr.Split(' ').Last().Trim();
        return tree.Find(l => l.name() == name &&
                              l as CEnum != null) != null ||
               Compiler.Enums[name]?["type"] != null;
    }
}
