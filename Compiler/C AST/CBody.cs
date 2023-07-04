namespace lang; 

public class CBody : Tree, Leaf {
    private List<Leaf> _leaves = new List<Leaf>();
    
    private string _name;
    
    public List<Leaf> leaves() => _leaves;
    public List<string> ImportPaths = new List<string>();
    public OptimizeType OptimizeType = OptimizeType.Medium;

    public Tree? tree { get; set; }

    public string name() => _name;
    public string type() => "body";

    public CBody(string name, Tree tree) {
        this._name = string.Join(" ", name.Split(new char[] { '\\', '/' }));
        this.tree = tree; 
        tree.leaves().Add(this);
    }
}