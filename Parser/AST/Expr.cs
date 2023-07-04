namespace lang; 

public class Expr {
    public class Literal { }
    public class Bool { }
    public class Null { }
    public class LambdaIdentifier { }
    public class LambdaExpr { }
    public class AssignExpr { }
    public class UnaryExpr { }
    public class InvokeExpr { }
    public class StaticInvokeExpr { }
    public class BinaryExpr { }

    public static Node ResolveLiteral(Token src) { return new Node(src, new Literal()); }
    public static Node ResolveBool(Token src) { return new Node(src, new Bool()); }
    public static Node ResolveNull(Token src) { return new Node(src, new Null()); }

    public static Node ResolveNumber(Token src) {
        object hold;
        if (src.type() == "int")
            hold = int.Parse(src.name());
        else if(src.type() == "long")
            hold = long.Parse(src.name());
        else if (src.type() == "float")
            hold = float.Parse(src.name());
        else
            hold = src.name();

        return new Node(src, hold);
    }

    public static Node ResolveBinary(Node lhv, Token op, Node rhv) {
        Node node = new Node(op, new BinaryExpr());
        node.Add(lhv, rhv);

        return node;
    }
    
    public static Node ResolveLambda(Node lhv, Token op, Node rhv) {
        Node node = new Node(op, new LambdaExpr());
        node.Add(lhv, rhv);

        return node;
    }

    public static Node ResolveInvoke(Node lhv, Token op, Node rhv) {
        Node node = new Node(op, new InvokeExpr());
        node.Add(lhv, rhv);

        return node;
    }
    
    public static Node ResolveStaticInvoke(Node lhv, Token op, Node rhv) {
        Node node = new Node(op, new StaticInvokeExpr());
        node.Add(lhv, rhv);

        return node;
    }
    
    public static Node ResolveUnaryExpr(Token op, Node rhv) {
        Node node = new Node(op, new UnaryExpr());
        node.Add(rhv);

        return node;
    }
    
    public static Node ResolveAssign(Node lhv, Token op, Node rhv) {
        Node node;
        node = new Node(op, new AssignExpr());
        node.Add(lhv, rhv);

        return node;
    }
    
    // Expr : Lambda
    public static Node Resolve(Parser P) {
        return Lambda(P);
    }

    public static Node Lambda(Parser P) {
        Node expr = LogicOr(P);
        
        if (P.current() == "double_arrow") {
            if(expr.Hold as Identifier == null)
                P.Error("Invalid lambda expression.", expr);
            
            Token op = P.Eat("double_arrow");
            Node rhv = LogicOr(P);
            expr = Expr.ResolveLambda(expr, op, rhv);
        }

        return expr;
    }

    public static Node LogicOr(Parser P) {
        Node expr = LogicAnd(P);
        while (P.current() == "or") {
            Token op = P.Eat("or");
            Node rhv = LogicAnd(P);
            expr = Expr.ResolveBinary(expr, op, rhv);
        }

        return expr;
    }
    
    public static Node LogicAnd(Parser P) {
        Node expr = LogicNot(P);
        while (P.current() == "and") {
            Token op = P.Eat("and");
            Node rhv = LogicNot(P);
            expr = Expr.ResolveBinary(expr, op, rhv);
        }

        return expr;
    }
    
    public static Node LogicNot(Parser P) {
        if (P.current() == "not") {
            Token op = P.Eat(P.current().type());
            Node rhv = LogicNot(P);
            return Expr.ResolveUnaryExpr(op, rhv);
        }

        return Equality(P);
    }
    
    // Equality : comparison (( "!=" | "==" ) comparison)* 
    public static Node Equality(Parser P) {
        Node expr = Comparison(P);

        while (P.current() == "compare") {
            Token op = P.Eat("compare");
            Node rhv = Comparison(P);
            expr = Expr.ResolveBinary(expr, op, rhv);
        }

        return expr;
    }

    // Comparison : term (( ">" | ">=" | "<" | "<=" ) term)*
    public static Node Comparison(Parser P) {
        Node expr = Term(P);
        string[] comparisons = new[] { "bigger", "bigger_eq", "smaller", "smaller_eq" };
        while (comparisons.Contains(P.current().type())) {
            Token op = P.Eat(P.current().type());
            Node rhv = Term(P);
            expr = Expr.ResolveBinary(expr, op, rhv);
        }

        return expr;
    }

    // Term : factor (( "-" | "+" ) factor)*
    public static Node Term(Parser P) {
        Node expr = Factor(P);
        string[] comparisons = new[] { "+", "-" };
        while (comparisons.Contains(P.current().name())) {
            Token op = P.Eat("operator");
            Node rhv = Factor(P);
            expr = Expr.ResolveBinary(expr, op, rhv);
        }

        return expr;
    }
    

    // Factor : unary (( "/" | "*" ) unary)*
    public static Node Factor(Parser P) {
        Node expr = Pow(P);
        string[] comparisons = new[] { "/", "*", "%" };
        while (comparisons.Contains(P.current().name())) {
            Token op = P.Eat("operator");
            Node rhv = Pow(P);
            expr = Expr.ResolveBinary(expr, op, rhv);
        }

        return expr;
    }
    
    // Pow : unary (^ unary)*    
    public static Node Pow(Parser P) {
        Node expr = Index(P);
        while (P.current().name() == "^") {
            Token op = P.Eat("operator");
            Node rhv = Index(P);
            expr = Expr.ResolveBinary(expr, op, rhv);
        }

        return expr;
    }

    public static Node Index(Parser P) {
        Node expr = Invoke(P);
        if (P.current() == "larray") 
            expr = Array.ResolveIndex(P, expr);
        
        return expr;
    }
    
    public static Node Invoke(Parser P) {
        Node expr = Unary(P);
        while (P.current() == "dot") {
            Token op = P.Eat("dot");
            Node rhv = Unary(P);
            expr = Expr.ResolveInvoke(expr, op, rhv);
        }

        return expr;
    }

    // Unary : ( "!" | "-" ) unary
    //       | primary
    public static Node Unary(Parser P) {
        if (P.current() == "not" || P.current().name() == "-") {
            Token op = P.Eat(P.current().type());
            Node rhv = Unary(P);
            return Expr.ResolveUnaryExpr(op, rhv);
        }
        else if (P.current() == "new") {
            Token op = P.Eat("new");
            Node rhv = Identifier.Resolve(P);
            return Expr.ResolveUnaryExpr(op, rhv);
        }

        return Primary(P);
    }

    // Unary : NUMBER | STRING | "true" | "false" | "nil"
    //       | "(" Expr ")" ;
    public static Node Primary(Parser P) {
        if (P.current() == "bool") {
            Token src = P.Eat(P.current().type());
            return ResolveBool(src);
        }

        if (P.current() == "null") {
            Token src = P.Eat(P.current().type());
            return ResolveNull(src);
        }
        
        if (P.current() == "string") {
            Token src = P.Eat(P.current().type());
            return ResolveLiteral(src);
        }
        if (P.current() == "int" || P.current() == "float" || P.current() == "long")  {
            Token src = P.Eat(P.current().type());
            return ResolveNumber(src);
        }

        if (P.current() == "lparan") {
            P.Eat("lparan");
            Node expr = Expr.Resolve(P);
            P.Eat("rparan");
            return expr;
        }

        return Atom(P);
    }
    
    // Atom : Variable
    //      | Variable definition
    //      | Call
    //      | Array
    public static Node Atom(Parser P) {
        Node node;
        if (P.current() == "ident") {
            node = Identifier.Resolve(P);
            return node;
        }

        else if (P.current() == "lbracket") {
            node = Array.Resolve(P);
            return node;
        }

        if(!Line.ExprErrors.ContainsKey(P.current().type()))
            P.Error($"Unexpected token: {P.current().name()}, expected an expression.");
        else 
            P.Error($"{Line.ExprErrors[P.current().type()]}");
        return null!;
    }
}
