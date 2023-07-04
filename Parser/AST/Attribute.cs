namespace lang; 

public class Attribute {
    public class Attributes { }
    
    public static Node Resolve(Parser P) {
        Node attributes = new Node(P.current(), new Attributes());
        while (P.current() == "attribute") 
            attributes.Add(new Node(P.Eat("attribute"), new Attribute()));

        Node next;
            
        switch (P.current().type()) {
            case "class":
                next = Class.Resolve(P);
                break;
                
            case "fn":
                next = Function.Resolve(P);
                break;
            
            case "enum":
                next = EnumStmt.Resolve(P);
                break;
                
            case "let":
                next = Variable.Resolve(P);
                break;
                
            default:
                P.Error($"Attributes can't be added to the following member: {P.current().type()}.");
                return null;
        }
            
        next.Insert(0, attributes);
        return next;
    }
}