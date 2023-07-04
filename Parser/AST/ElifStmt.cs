namespace lang; 

public class ElifStmt {
    // ElifStmt : else if "(" Expr ")" : (block | line) (elif *) ( else )
    public static List<Node> Resolve(Parser P) {
        List<Node> elifstmts = new List<Node>();
        
        while (P.current() == "elif") {
            Node elifstmt = new Node(P.current(), new ElifStmt());
            elifstmts.Add(elifstmt);
            P.Eat("elif");
            P.Eat("lparan");
            Node condition = Expr.Resolve(P);
            elifstmt.Add(condition);
            P.Eat("rparan");
        
            if (P.current() == "lbracket") 
                elifstmt.Add(Block.Resolve(P));
            else 
                elifstmt.Add(Line.Resolve(P));
        }

        if (P.current() == "else") 
            elifstmts.Add(ElseStmt.Resolve(P));
        
        return elifstmts;
    }
}