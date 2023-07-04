namespace lang; 

public class CRule : Tree, Leaf {
    private List<Leaf> _leaves = new List<Leaf>();
    
    private string _name;
    
    public List<Leaf> leaves() => _leaves;
    public Tree? tree {get; set;}
    public string name() => _name;
    public string type() => "rule";
    public CEvent Event;
    public List<CCondition> Conditions = new List<CCondition>();

    public CRule(string name, CEvent Event, Tree tree) {
        this._name = name;
        this.Event = Event;
        this.tree = tree;
        tree.leaves().Add(this);
    }

    public static CRule Unpack(Node rnode, Tree tree) {
        Node name = rnode.Consume<Expr.Literal>()!,
            Event = rnode.Consume<Rule.Event>()!,
            conditions = rnode.Consume<Rule.Condition.Conditions>()!,
            actions = rnode.Consume<Block>()!;

        CRule rule = new CRule(name.source.name(), new CEvent(Event), tree);
        foreach (Node condition in conditions.Children) 
            rule.Conditions.Add(new CCondition(condition.Get(0)!, rule));

        foreach (Node action in actions.Children) 
            CLine.Unpack(action, rule, CLine.LineType.Rule);

        return rule;
    }
}
