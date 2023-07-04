namespace lang; 

public class Variable {
    public class VariableStatement { }

    public static Node Resolve(Parser P) {
        Node statement = new Node(P.Eat("let"), new VariableStatement());
        return ResolveVariable(P, ref statement);
    }
    
    public static Node ResolveVariable(Parser P, ref Node statement) {
        Node ident, type;
        bool StaticLet;
        if (P.Peek() == "colon") {
            StaticLet = false;
            ident = new Node(P.Eat("ident"), new Identifier());
            P.Eat("colon");
            type = new Node(P.Eat("ident", "type"), new object());
            if (type.source == "ident")
                type.Hold = new Identifier();
            else
                type.Hold = new Identifier.Type();
            if (P.current() == "larray")
                Identifier.Type.ResolveArrayType(P, ref type);
        }
        else {
            StaticLet = true;
            type = new Node(P.Eat("ident", "type"), new object());
            if (type.source == "ident")
                type.Hold = new Identifier();
            else
                type.Hold = new Identifier.Type();
            if (P.current() == "larray")
                Identifier.Type.ResolveArrayType(P, ref type);
            ident = new Node(P.Eat("ident"), new Identifier());
        }

        statement.Add(ident, type);
        if (P.current() == "assign") {
            P.Eat("assign");
            Node initvalue = Expr.Resolve(P);
            statement.Add(initvalue);
        }

        while (P.current() == "comma") {
            P.Eat("comma");
            if (!StaticLet) {
                ident = new Node(P.Eat("ident"), new Identifier());
                P.Eat("colon");
                type = new Node(P.Eat("ident", "type"), new object());
                if (type.source == "ident")
                    type.Hold = new Identifier();
                else
                    type.Hold = new Identifier.Type();
                if (P.current() == "larray")
                    Identifier.Type.ResolveArrayType(P, ref type);
            }
            else 
                ident = new Node(P.Eat("ident"), new Identifier());

            statement.Add(ident, type);
            if (P.current() == "assign") {
                P.Eat("assign");
                Node initvalue = Expr.Resolve(P);
                statement.Add(initvalue);
            }
        }

        P.Eat("semicolon");
        return statement;
    }
}