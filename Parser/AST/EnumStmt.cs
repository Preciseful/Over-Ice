namespace lang; 

public class EnumStmt {
    public static Node Resolve(Parser P) {
        Node enumstmt = new Node(P.Eat("enum"), new EnumStmt()),
            ident = new Node(P.Eat("ident"), new Identifier()), 
            block = new Node(P.Eat("lbracket"), new Block());

        int index = 0;
        while (P.current() != "rbracket") {
            Node sub = new Node(P.Eat("ident"), new Identifier());
            sub.Add(new Node(sub.source, index));
            block.Add(sub);
            index++;
            if (P.current() != "comma") 
                break;
            P.Eat("comma");
        }

        P.Eat("rbracket");
        enumstmt.Add(ident, block);
        return enumstmt;
    }
}