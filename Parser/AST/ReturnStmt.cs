namespace lang; 

public class ReturnStmt {
    // Return : return ( expr ) semicolon
    public static Node Resolve(Parser P) {
        Node node = new Node(P.current(), new ReturnStmt());
        P.Eat("return");
        Node expr = Expr.Resolve(P);
        P.Eat("semicolon");
        node.Add(expr);
        return node;
    }
}