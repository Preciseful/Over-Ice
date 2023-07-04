namespace lang; 

public class Unoptimize {
    public static Node Resolve(Parser P) {
        Node unoptimize = new Node(P.Eat("unoptimize"), new Unoptimize());
        unoptimize.Add(Block.Resolve(P));

        return unoptimize;
    }
}