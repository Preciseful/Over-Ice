namespace lang; 

public class IfStmt {
    // IfStmt : if "(" Expr ")" : (block | line) (elif *) ( else )
    public static Node Resolve(Parser P) {
        Node ifstmt = new Node(P.current(), new IfStmt());
        P.Eat("if");
        P.Eat("lparan");
        Node condition = Expr.Resolve(P);
        ifstmt.Add(condition);
        P.Eat("rparan");
        
        if (P.current() == "lbracket") 
            ifstmt.Add(Block.Resolve(P));
        else 
            ifstmt.Add(Line.Resolve(P));

        List<Node> nodes = ElifStmt.Resolve(P);
        ifstmt.Add(nodes);

        return ifstmt;
    }
}