namespace lang;

class Program {
    public static void Main(string[] args) {
        Compiler compiler = new Compiler(args[0]);
        compiler.Compile();
    }
}