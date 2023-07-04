namespace lang; 

public class Call {
    public class AccessParameter { };
    public class Arguments { };

    public static Node Resolve(Parser P) {
        Node node = new Node(P.Eat("ident"), new Call());
        return Resolve(P, ref node);
    }
    
    public static Node Resolve(Parser P, ref Node node) {
        node = new Node(node.source, new Call());
        node.Add(ResolveArguments(P).Children);

        return node;
    }

    public static Node ResolveArguments(Parser P) {
        Node args = new Node(P.Eat("lparan"), new Arguments());
        Node node;
        while (P.current() != "rparan") {
            if (P.current() == "ident" && P.Peek() == "colon") {
                node = new Node(P.Eat("ident"), new AccessParameter());
                P.Eat("colon");
                node.Add(Expr.Resolve(P));
            }
            else    
                node = Expr.Resolve(P);
            args.Add(node);
            if (P.current() == "rparan")
                break;
            P.Eat("comma");
        }

        P.Eat("rparan");
        return args;
    }
}
