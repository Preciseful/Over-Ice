namespace lang; 

public class ElseStmt {
    // ElseStmt : else : (block | line)
    public static Node Resolve(Parser P) {
        Node elsestmt = new Node(P.current(), new ElseStmt());
        P.Eat("else");
            
        if (P.current() == "lbracket") 
            elsestmt.Add(Block.Resolve(P));
        else 
            elsestmt.Add(Line.Resolve(P));
        
        return elsestmt;
    }
}