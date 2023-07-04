namespace lang; 

public class CAttribute {
    public string name;
    
    public CAttribute(string name) {
        this.name = name;
    }

    public static List<CAttribute> Unpack(Node? attrs) {
        if (attrs == null)
            return new List<CAttribute>();
        
        List<CAttribute> cAttributes = new List<CAttribute>();
        foreach (Node node in attrs.Children) 
            cAttributes.Add(new CAttribute(node.source.name()));
        return cAttributes;
    }
}