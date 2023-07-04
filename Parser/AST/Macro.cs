namespace lang; 

public class Macro {
    private static string EnvVariable = "";
    
    private static string ReadUniversalImport(Parser P) {
        if (EnvVariable == "") {
            EnvVariable = Environment.GetEnvironmentVariable("DEW_INCLUDE") ?? "";
            
            if(EnvVariable == "")
                P.Error("Environment variable \"DEW_INCLUDE\" is not set. Check the wiki for more info.");
        }

        string path = "";
        while (true) {
            Token next = P.Eat(P.current().type());
            if (next == "bigger")
                break;
            
            path += next.name();
        }

        return path.Insert(0, EnvVariable);
    }

    private static string ReadCurrentImport(string prepath, string file) {
        List<string> paths = prepath.Split("\\/".ToCharArray()).ToList(),
            filepaths = file.Split("\\/".ToCharArray()).ToList();
        
        filepaths.RemoveAt(filepaths.Count - 1);
        
        while(paths[0] == "..") {
            paths.RemoveAt(0);
            filepaths.RemoveAt(filepaths.Count - 1);
        }
        
        filepaths.AddRange(paths);
        return string.Join("/", filepaths);
    }
    
    // Macro: #( ident ) ( arg * )
    public static Node Resolve(Parser P) {
        P.Eat("hashtag");
        Token header = P.Overhead("import", "declare", "optimize");
        header._type = "macro";
        Node node = new Node(header, new Macro());

        switch (header.name()) {
            case "import":
                Token ImportType = P.Eat("smaller", "string");
                string ImportPath;
                if (ImportType == "string")
                    ImportPath = ReadCurrentImport(ImportType.name().Substring(1, ImportType.name().Length - 2), ImportType.file.FullName);
                else
                    ImportPath = ReadUniversalImport(P);
                Node import = new Node(ImportType, ImportPath);
                node.Add(import);
                if (!File.Exists(ImportPath))
                    P.Error($"File \"{ImportPath}\" doesn't exist.");
                
                P.Import(ImportPath);
                break;
            
            case "optimize":
                Token type = P.Overhead("max", "min", "none");
                type._type = "optimize type";
                node.Add(Expr.ResolveLiteral(type));
                break;

            case "declare":
                throw new NotImplementedException();
        }

        return node;
    }
}