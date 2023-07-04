namespace lang; 

public class CString : Leaf {
    public Tree? tree { get; set; }
    private string _name;

    public string name() => _name;
    public string type() => "string";

    public CString(string name, Tree? tree = null) {
        this._name = name;
        this.tree = tree;
        tree?.leaves().Add(this);
    }
}