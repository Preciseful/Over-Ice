namespace lang; 

public class Array {
    public class IndexAccess { }
    
    // Array : "{" (, primary * ) "}"
    public static Node Resolve(Parser P) {
        Node node = new Node(P.current(), new Array());
        P.Eat("lbracket");
        while (P.current() != "rbracket") {
            node.Add(Expr.Primary(P));
            
            if (P.current() == "rbracket")
                break;
            P.Eat("comma");
        }

        P.Eat("rbracket");
        return node;
    }
    
    // Index : ( Identifier) [ Term ] 
    public static Node ResolveIndex(Parser P, Node ident) {
        Node node = new Node(P.current(), new IndexAccess());
        node.Add(ident);
        P.Eat("larray");
        node.Add(Expr.Term(P));
        P.Eat("rarray");
        return node;
    }
}