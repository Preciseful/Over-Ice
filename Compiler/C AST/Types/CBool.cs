namespace lang; 

public class CBool : Leaf {
    private string _name;

    public Tree? tree {get; set;}
    public string name() => _name;
    public string type() => "bool";
    public bool value;

    public CBool(string name, bool value, Tree? tree = null) {
        this.value = value;
        this._name = name;
        this.tree = tree;
        tree?.leaves().Add(this);
    }
}