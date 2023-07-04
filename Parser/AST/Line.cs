namespace lang; 

public class Line {
    // Line :
    //      ( WhileStmt
    //      | IfStmt
    //      | ForStmt
    //      | VarStmt
    //      | FnStmt
    //      | ClassStmt
    //      | EnumStmt
    //      | NamespaceStmt
    //      | Call
    //      | ReturnStmt
    //      | Expr
    //      | Expr assign Expr
    //      )? semicolon
    public static Node Resolve(Parser P, Block.Type type = Block.Type.Normal) {
        switch (P.current().type()) {
            case "semicolon":
                Token semicolon = P.Eat("semicolon");
                return new Node(semicolon, Expr.ResolveLiteral(semicolon));
            case "if":
                return IfStmt.Resolve(P);
            case "while":
                return WhileStmt.Resolve(P);
            case "for":
                return ForStmt.Resolve(P);
            case "return":
                return ReturnStmt.Resolve(P);
            case "let":
                if (type == Block.Type.While) 
                    goto default;
                return Variable.Resolve(P);
            case "fn":
                if (type == Block.Type.While || type == Block.Type.For) 
                    goto default;
                return Function.Resolve(P);
            case "attribute":
                return Attribute.Resolve(P);
            case "unoptimize":
                return Unoptimize.Resolve(P);

            case "class":
                if (type != Block.Type.Class && type != Block.Type.Namespace) 
                    goto default;
                return Class.Resolve(P);
            
            case "namespace":
                if (type != Block.Type.Namespace)
                    goto default;
                return Namespace.Resolve(P);
            
            default:
                if(BlockErrors.ContainsKey(P.current().type()))
                    P.Error(BlockErrors[P.current().type()]);
                break;
        }

        Node Line = Expr.Resolve(P);
        if (P.current() == "assign") {
            Token op = P.Eat("assign");
            Line = Expr.ResolveAssign(Line, op, Expr.Resolve(P));
        }
        
        // if it contains a block, we don't need a semicolon
        if(!Line.Children.Exists(c => c.Hold as Block != null))
            P.Eat("semicolon");
        return Line;
    }

    public static Dictionary<string, string> BlockErrors = new Dictionary<string, string>() {
        { "rule",      "Rules can't be contained within blocks." }, 
        { "hashtag",   "Macros must be declared at rule-level." },
        { "namespace", "Namespaces can only be contained in other namespaces." },
        { "class",     "Classes can only be contained in other classes or namespaces." }
    };

    public static Dictionary<string, string> ExprErrors = new Dictionary<string, string>() {
        { "else", "\"Else\" can only follow after an \"if\" or \"else if\"." },
        { "elif", "\"Else if\" can only follow after an \"if\" or another \"else if\"." },
    };
}
