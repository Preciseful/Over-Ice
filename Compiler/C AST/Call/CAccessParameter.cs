namespace lang; 

public class CAccessParameter : Leaf {
    private string _name, _type;
        
    public Tree? tree { get; set; }
    public string name() => _name;
    public string type() => _type;
    public Leaf Expr;
    public int index;

    public CAccessParameter(string _name, string _type, Leaf Expr, Tree tree) {
        this.tree = tree;
        this.Expr = Expr;
        tree.leaves().Add(this);
        this._name = _name;
        this._type = _type;
    }
}