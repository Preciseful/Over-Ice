namespace lang; 

public class ForStmt {
    // ForStmt : for "(" Line? ; Expr? ; Expr? ; ")" : (block | line)
    public static Node Resolve(Parser P) {
        Node forstmt = new Node(P.current(), new ForStmt());
        P.Eat("for");
        P.Eat("lparan");

        if (P.current() == "semicolon") {
            forstmt.Add(Expr.ResolveLiteral(P.current()));
            P.Eat("semicolon");
        }
        else
            forstmt.Add(Line.Resolve(P, Block.Type.For));

        try {
            forstmt.Add(P.current() == "semicolon" 
                ? Expr.ResolveLiteral(P.current()) 
                : Expr.Resolve(P));
            
            P.Eat("semicolon");
            
            forstmt.Add(P.current() == "semicolon" 
                ? Expr.ResolveLiteral(P.current())
                : Expr.Resolve(P));

            if (P.current() == "semicolon")
                P.Eat("semicolon");
        }
        catch {
            P.Error("Expression expected.");
        }

        P.Eat("rparan");
        
        
        if (P.current() == "lbracket") 
            forstmt.Add(Block.Resolve(P));
        else 
            forstmt.Add(Line.Resolve(P));

        return forstmt;
    }
}