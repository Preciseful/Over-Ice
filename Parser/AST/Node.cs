namespace lang; 

public class Body { }
public class Node {
    public Token source;
    public object Hold;
    public List<Node> Children = new List<Node>();
    public Node? Parent;

    public Node(Token source, object Hold) {
        this.Hold = Hold;
        this.source = source;
    }

    public void Add(params Node[] nodes) {
        Children.AddRange(nodes);
        foreach (Node node in nodes) 
            node.Parent = this;
    }
    
    public void Add(params List<Node>[] listnodes) {
        foreach (var nodes in listnodes) {
            Children.AddRange(nodes);
            foreach (Node node in nodes) 
                node.Parent = node;
        }
    }
    
    public void Insert(int index, params Node[] nodes) {
        Children.InsertRange(index, nodes);
        foreach (Node node in nodes) 
            node.Parent = this;
    }

    public void Insert(int index, params List<Node>[] listnodes) {
        foreach (var nodes in listnodes) {
            Children.InsertRange(index, nodes);
            foreach (Node node in nodes) 
                node.Parent = node;
        }
    }

    public Node? Get<T>() where T : class {
        return Children.Find(c => c.Hold as T != null);
    }

    public Node? Consume<T>() where T : class {
        Node? node = Children.Find(c => c.Hold as T != null);
        if (node != null) Children.Remove(node);
        return node;
    }
    
    public Node? Consume<T, U>() where T : class where U : class {
        Node? node = Children.Find(c => c.Hold as T != null || c.Hold as U != null);
        if (node != null) Children.Remove(node);
        return node;
    }
    
    public Node? Consume<T, U, V>() where T : class where U : class where V : class {
        Node? node = Children.Find(c => c.Hold as T != null || c.Hold as U != null || c.Hold as V != null);
        if (node != null) Children.Remove(node);
        return node;
    }

    public Node? Consume(int index) {
        if (index > Children.Count - 1)
            return null;
        Node node = Children[index];
        Children.Remove(node);
        return node;
    }
    
    public Node? Get<T>(Predicate<Node> predicate) where T : class {
        List<Node> children = Children.FindAll(c => c.Hold as T != null);

        foreach (var child in children) 
            if (predicate.Invoke(child))
                return child;

        return null;
    }
    
    public Node? Get(int index) {
        if (index > Children.Count - 1)
            return null;
        return Children[index];
    }
}