namespace lang; 
using Newtonsoft.Json.Linq;

public class COperator : Leaf {
    private string _name;
    public Tree? tree { get; set; }

    public string name() => _name;
    public string type() => "operator";
    
    public COperator(string name, Tree? tree = null) {
        this._name = name;
        this.tree = tree;
        tree?.leaves().Add(this);
    }

    public static COperator Unpack(string name, string backupType, Tree? tree = null) {
        return new COperator(name, tree);
    }

    public static void AddOperands(string x, string y, string op, string ret) {
        Compiler.Operations.TryAdd(x, new JObject());
        Compiler.Operations[x]!.ToObject<JObject>()!.TryAdd(op, new JObject());
        Compiler.Operations[x]![op]!.ToObject<JObject>()!.TryAdd(y, ret);
    }
    
    public bool VerifyTypes(string x, string y) {
        bool contains = Compiler.Operations[x]?[this.name()]?[y] != null;
        if(!contains)
            contains = Compiler.Operations[y]?[this.name()]?[x] != null;
        if (!contains && (this.name() == "==" || this.name() == "!=") && x == y)
            return true;
        
        return contains;
    }

    public string GetOperandReturn(string x, string y) {
        if (this.name() == "==" || this.name() == "!=")
            return "bool";
        
        string? contains = Compiler.Operations[x]?[this.name()]?[y]?.ToString();
        if(contains == null)
            contains = Compiler.Operations[x]![this.name()]![y]!.ToString()!;
        return contains;
    }
}