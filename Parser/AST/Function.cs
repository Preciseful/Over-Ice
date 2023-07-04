namespace lang; 

public class Function {
    public class FunctionStatement { }
    public class Parameters { }
    public class DirectReturn { }

    // This defines the function
    // Function : fn Identifier  Arguments -> Type block
    public static Node Resolve(Parser P) {
        Node def = new Node(P.Eat("fn"), new FunctionStatement());
        def.Add(new Node(P.Eat("ident"), new Identifier()));
        Node args = new Node(P.current(), new Parameters());
        ResolveArguments(P, ref args);

        P.Eat("arrow");
        Node type = new Node(P.Eat("ident", "type"), new object());
        if (type.source == "ident")
            type.Hold = new Identifier();
        else
            type.Hold = new Identifier.Type();
        def.Insert(1, type);
        
        if (P.current() == "double_arrow") {
            Node dirreturn = new Node(P.Eat("double_arrow"), new DirectReturn());
            Node lhv = Expr.Resolve(P);
            dirreturn.Add(lhv);
            
            if (P.current() == "assign") {
                P.Eat("assign");
                Node rhv = Expr.Resolve(P);
                dirreturn.Add(rhv);
            }
            
            P.Eat("semicolon");
            def.Add(args, dirreturn);
            return def;
        }
        
        def.Add(args, Block.Resolve(P));
        return def;
    }
    
    // Arguments :  ( , arg * )
    public static void ResolveArguments(Parser P, ref Node block) {
        P.Eat("lparan");
        while (P.current() != "rparan") {
            List<Node> ident = ResolveArg(P);
            Node statement = new Node(ident[0].source, new Variable.VariableStatement());
            statement.Add(ident);
            block.Add(statement);
            if (P.current() == "rparan")
                break;
            P.Eat("comma");
        }

        P.Eat("rparan");
    }

    // arg: Identifier : Type ( = (Compile-Constant)Expr )
    public static List<Node> ResolveArg(Parser P) {
        List<Node> nodes = new List<Node>();
        Node ident = new Node(P.Eat("ident"), new Identifier()),
            attrs = new Node(P.current(), new Attribute.Attributes());

        if (P.current().name() == "*" || P.current().name() == "&") 
            attrs.Add(new Node(P.Eat(P.current().type()), new Attribute()));

        P.Eat("colon");
        Node type = new Node(P.Eat("ident", "type"), new Identifier.Type());
        
        nodes.Add(attrs);
        nodes.Add(ident);
        nodes.Add(type);
        if (P.current() == "assign") {
            P.Eat("assign");
            Node initvalue = Expr.Resolve(P);
            nodes.Add(initvalue);
        }

        return nodes;
    }
}