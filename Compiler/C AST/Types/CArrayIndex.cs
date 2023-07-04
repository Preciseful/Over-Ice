namespace lang; 

public class CArrayIndex : Leaf {
    public Tree? tree { get; set; }

    private string _type;

    public Leaf access, index;
    public string name() => "index";
    public string type() => _type;

    public CArrayIndex(string type, Leaf Access, Leaf Index, Tree tree) {
        this.tree = tree;
        tree.leaves().Add(this);
        this._type = type;
        this.access = Access;
        this.index = Index;
    }
}
