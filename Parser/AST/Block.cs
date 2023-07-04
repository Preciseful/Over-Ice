namespace lang; 

public class Block {
    // block : LBRACKET line RBRACKET
    //       | line
    public static Node Resolve(Parser P, Type type = Type.Normal) {
        Node Block = new Node(P.current(), new Block());

        if (P.current() == "lbracket") {
            P.Eat("lbracket");
            while (P.current() != "rbracket") 
                Block.Add(Line.Resolve(P, type));

            P.Eat("rbracket");
        }
        else 
            Block.Add(Line.Resolve(P, type));

        return Block;
    }

    public enum Type {
        For,
        While,
        Normal,
        Class,
        Namespace
    }
}