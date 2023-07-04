namespace lang; 

public class Identifier {
    public class ArrayType { }

    public class Type {
        public static Node ResolveArrayType(Parser P, ref Node type) {
            Node node = new Node(P.current(), new ArrayType()),
                parent = node;
            P.Eat("larray");

            while (P.current() == "comma") {
                node.Add(new Node(P.Eat("comma"), new ArrayType()));
                node = node.Children[0];
            }

            node.Add(type);
            type = parent;
            P.Eat("rarray");
            return parent;
        } 
    }

    public static Node Resolve(Parser P) {
        Node node = new Node(P.Eat("ident"), new Identifier());
        while (P.current() == "double_colon") {
            Token op = P.Eat("double_colon");
            Node rhv = Expr.Atom(P);
            node = Expr.ResolveStaticInvoke(node, op, rhv);
        }
        
        if (P.current() == "lparan") 
            return Call.Resolve(P, ref node);

        // also in case we increment or decrement the variable
        if (P.current() == "increment" || P.current() == "decrement") {
            Node crement = Expr.ResolveUnaryExpr(P.Eat(P.current().type()), node);
            crement.Add(node);
            return crement;
        }
        
        return node;
    }
}