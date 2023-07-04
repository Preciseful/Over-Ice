namespace lang; 

public interface Leaf {
    public Tree? tree { get; set; }
    public string name();
    public string type();
}