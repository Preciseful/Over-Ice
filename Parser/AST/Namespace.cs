namespace lang; 

public class Namespace {
    // Namespace : namespace ( Identifier ) block
    public static Node Resolve(Parser P) {
        Token classtoken = P.Eat("namespace"), name = P.Eat("ident");
        Node NamespaceNode = new Node(classtoken, new NamespaceStatement()),
            NameNode = Expr.ResolveLiteral(name),
            Block;
        Block = lang.Block.Resolve(P, lang.Block.Type.Namespace);

        NamespaceNode.Add(NameNode, Block);
        
        return NamespaceNode;
    }

    public class NamespaceStatement { }
}