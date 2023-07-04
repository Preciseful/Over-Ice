namespace lang; 

public class CInt : Leaf {
    public Tree? tree { get; set; }
    private string _name;

    public string name() => _name;
    public string type() => "int";
    public int value;

    public CInt(string name, int value, Tree? tree = null) {
        this.value = value;
        this._name = name;
        this.tree = tree;
        tree?.leaves().Add(this);
    }
}