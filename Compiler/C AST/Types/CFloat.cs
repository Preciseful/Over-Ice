namespace lang; 

public class CFloat : Leaf {
    public Tree? tree { get; set; }
    private string _name;

    public string name() => _name;
    public string type() => "float";
    public float value;

    public CFloat(string name, float value, Tree? tree = null) {
        this.value = value;
        this._name = name;
        this.tree = tree;
        tree?.leaves().Add(this);
    }
}