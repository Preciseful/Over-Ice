namespace lang; 

public class WhileStmt {
    // WhileStmt : while "(" Expr ")" : (block | line)
    public static Node Resolve(Parser P) {
        Node whilestmt = new Node(P.current(), new WhileStmt());
        P.Eat("while");
        P.Eat("lparan");
        whilestmt.Add(Expr.Resolve(P));
        P.Eat("rparan");
        
        if (P.current() == "lbracket") 
            whilestmt.Add(Block.Resolve(P, Block.Type.While));
        else 
            whilestmt.Add(Line.Resolve(P, Block.Type.While));

        return whilestmt;
    }
}