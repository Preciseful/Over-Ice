namespace lang; 

public static class CMacro {
    public static void Unpack(Node node, Tree tree) {
        if(tree as CBody == null)
            Compiler.Error("Macro is not defined at rule-level.", node);
        
        switch (node.source.name()) {
            case "import":
                string importhold = (string)node.Consume(0)!.Hold;
                ((CBody)tree).ImportPaths.Add(string.Join(" ", importhold.Split(new char[] { '\\', '/'})));
                break;
        }
    }
}