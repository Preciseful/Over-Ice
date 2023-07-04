namespace lang; 

public class CNull : Leaf {
    public Tree? tree {get; set;}
    public string name() => "null";
    public string type() => "null";
    public int value;

    public CNull(Tree? tree = null) {
        this.tree = tree;
        tree?.leaves().Add(this);
    }
}