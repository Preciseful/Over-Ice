using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace lang;

public enum OptimizeType {
    High,
    Low,
    Medium,
    None
}

public class Compiler {
    public string MainFile;
    private Parser P;
    //todo: change
    private const string wspath = @"F:\a\lang\lang\Workshop.json";

    public static JObject Workshop = JObject.Parse(File.ReadAllText(wspath)),
        Actions = Compiler.Workshop["actions"]!.ToObject<JObject>()!,
        Values = Compiler.Workshop["values"]!.ToObject<JObject>()!,
        Events = Compiler.Workshop["events"]!.ToObject<JObject>()!,
        Enums = Compiler.Workshop["enums"]!.ToObject<JObject>()!,
        Operations = Compiler.Workshop["operations"]!.ToObject<JObject>()!;
    

    public Compiler(string file) {
        MainFile = file;
        Stopwatch parserwatch = new Stopwatch();
        parserwatch.Start();
        P = new Parser();
        P.Import(MainFile);
        P.Parse();
        parserwatch.Stop();
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        // Console.WriteLine($"\nParsing done in {parserwatch.Elapsed.ToString()}");

        /*Draw draw = new Draw();
        draw.DrawParser(P);*/
        Console.ForegroundColor = ConsoleColor.Red;
    }
    
    [DoesNotReturn]
    public static void Error(string message, Token src) {
        throw new Exception($"{message}\n\t at {src.line}:{src.col} in {src.file}");
    }
    
    [DoesNotReturn]
    public static void Error(string message, Node src) {
        throw new Exception($"{message}\n\t at {src.source.line}:{src.source.col} in {src.source.file}");
    }

    public void ReadStmts(Node read, Tree tree) {
        List<Node> stmts = read.Children.FindAll(c => c.Hold as EnumStmt != null ||
                                                      c.Hold as Macro != null);
       List<Node> enumstmts = stmts.FindAll(c => c.Hold as EnumStmt != null),
            macrostmts =  stmts.FindAll(c => c.Hold as Macro != null);
        
        macrostmts.ForEach(node => CMacro.Unpack(node, tree));
        enumstmts.ForEach(node => CEnum.Unpack(node, tree));
        read.Children.RemoveAll(c => enumstmts.Contains(c));
    }

    public void CompleteVars(Node read, Tree tree) {
        List<Node> varstmts = read.Children.FindAll(c => c.Hold as Variable.VariableStatement != null);
            
        varstmts.ForEach(node => CVariable.Unpack(node, tree));
        read.Children.RemoveAll(c => varstmts.Contains(c));
    }
    
    public void CompleteFuncs(Node read, Tree tree) {
        List<Node> fstmts = read.Children.FindAll(c => c.Hold as Function.FunctionStatement != null);
            
        fstmts.ForEach(node => CFunction.Unpack(node, tree));
        read.Children.RemoveAll(c => fstmts.Contains(c));
        
        List<CFunction> cFunctions = tree.leaves().FindAll(l => l as CFunction != null && ((CFunction)l).block != null).ConvertAll(l => (CFunction)l);
        foreach (CFunction function in cFunctions) {
            if (function.DirectReturn) {
                CExpr.Convert(function.block!.Children[0], function);
                continue;
            }

            foreach (Node node in function.block!.Children) 
                CLine.Unpack(node, function, CLine.LineType.Function);
            
            if(function.type() != "void" && !function.leaves().Exists(l => l as CReturn != null))
                Compiler.Error("Return statement is missing.", function.block);
        }
    }

    public void ReadRules(Node read, Tree tree) {
        List<Node> nodes = read.Children.FindAll(c => c.Hold as Rule != null);
        foreach (Node node in nodes) CRule.Unpack(node, tree);
    }
    
    public void Compile() {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        CProgram program = new CProgram();
        foreach (var body in P.Daddy) {
            CBody cbody = new CBody(body.source.file.FullName, program);
            ReadStmts(body, cbody);
        }
        
        for (int i = 0; i < program.leaves().Count; i++) {
            Tree cbody = (Tree)program.leaves()[i];
            Node body = P.Daddy[i];
            CompleteVars(body, cbody);
        }
        
        for (int i = 0; i < program.leaves().Count; i++) {
            Tree cbody = (Tree)program.leaves()[i];
            Node body = P.Daddy[i];
            CompleteFuncs(body, cbody);
        }
        
        for (int i = 0; i < program.leaves().Count; i++) {
            Tree cbody = (Tree)program.leaves()[i];
            Node body = P.Daddy[i];
            ReadRules(body, cbody);
        }
        
        stopwatch.Stop();
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        // Console.WriteLine($"\nCompiling done in {stopwatch.Elapsed.ToString()}\n\n");

        CWorkshop workshop = new CWorkshop(program);
        workshop.Start();
    }
}