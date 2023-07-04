namespace lang; 

public class Class {
    public class Constructor {
        public static Node ResolveConstructor(Parser P) {
            Node node = Function.Resolve(P);
            node.Hold = new Constructor();
            return node;
        }
    }
    
    // Class : class ( Identifier ) block
    public static Node Resolve(Parser P) {
        Token classtoken = P.Eat("class"), name = P.Eat("ident");
        Node ClassNode = new Node(classtoken, new ClassStatement()),
            NameNode = Expr.ResolveLiteral(name),
            Block = lang.Block.Resolve(P, lang.Block.Type.Class);
        
        ClassNode.Add(NameNode, Block);
        
        return ClassNode;
    }

    public class ClassStatement { }
}
