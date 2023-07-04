namespace lang; 

public class Squiggly {
    public static Node Resolve(Parser P) {
        Node attributes = new Node(P.current(), new Attribute.Attributes());
        P.Eat("squiggly");
        attributes.Add(new Node(P.Eat("ident"), new Attribute()));
        Node attr = attributes.Children.Last();
        attr.source._name = $"~{attr.source._name}";
            
        while (P.current() == "comma") {
            attributes.Add(new Node(P.Eat("ident"), new Attribute()));
            attr = attributes.Children.Last();
            attr.source._name = $"~{attr.source._name}";
        }

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

            default:
                P.Error($"Squiggly attributes can't be added to the following member: {P.current().type()}.");
                return null;
        }
            
        next.Insert(0, attributes);
        return next;
    }
}