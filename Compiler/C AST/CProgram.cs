namespace lang; 

public class CProgram : Tree {
    private List<Leaf> _leaves = new List<Leaf>();
    public List<Leaf> leaves() => _leaves;
    public string name() => "program";
}