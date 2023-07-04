namespace lang; 

public class Draw {
    public int ident = 1;
    
    public void DrawParser(Parser P) {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("\n\nProgram:");
        foreach (Node LastNode in P.Daddy) {
            Console.Write($"\n|------- Body \"{LastNode.source.file}\"\n");

            foreach (Node n in LastNode.Children) {
                ident++;
                for (int i = 0; i < ident - 1; i++)
                    Console.Write('\t');
                
                Console.Write("|------- ");
                bool block = false;
                if (n.Hold as Block == null)
                    Console.Write($"{n.source.type()} \"{n.source.name()}\" -> {n.Hold.GetType().Name}");
                else {
                    Console.Write($"START BLOCK");
                    block = true;
                }

                Console.Write('\n');
                RevealChildren(n);
                if (block) {
                    for (int i = 0; i < ident - 1; i++)
                        Console.Write('\t');
                    Console.Write("|------- END BLOCK");
                    Console.Write('\n');
                }

                ident--;
            }
        }
    }

    public void RevealChildren(Node node) {
        foreach (Node n in node.Children) {
            ident++;
            for (int i = 0; i < ident - 1; i++) 
                Console.Write("\t");
            Console.Write("|------- ");

            bool block = false;
            if(n.Hold as Block == null)
                Console.Write($"{n.source.type()} \"{n.source.name()}\" -> {n.Hold.GetType().Name}");
            else {
                Console.Write($"START BLOCK");
                block = true;
            }

            Console.Write('\n');
            RevealChildren(n);
            if (block) {
                for (int i = 0; i < ident - 1; i++) 
                    Console.Write('\t');
                Console.Write("|------- END BLOCK");
                Console.Write('\n');
            }

            ident--;
        }
    }
}