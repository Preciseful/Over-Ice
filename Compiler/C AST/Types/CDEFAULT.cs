namespace lang; 

public class CDEFAULT : Leaf {
    public Tree? tree {get; set;}
    public string name() => "DEFAULT";
    public string type() => "DEFAULT";
    public int value;

    public CDEFAULT(Tree? tree = null) {
        this.tree = tree;
        tree?.leaves().Add(this);
    }
}