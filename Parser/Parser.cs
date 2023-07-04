using System.Diagnostics.CodeAnalysis;

namespace lang; 

public class Parser {
    private int _position;
    private List<Token> Tokens = new List<Token>();
    private HashSet<string> Imports = new HashSet<string>();

    public int position() => _position;
    public int lastpos() => Tokens.Count - 1;
    public Token current() => Tokens[position()];
    public bool parse_ongoing() => current().type() != "eof";
    public Node Program = default!;
    public List<Node> Daddy = new List<Node>();

    [DoesNotReturn]
    public void Error(string message) {
        throw new Exception($"{message}\n\t at {current().line}:{current().col} in {current().file}");
    }
    
    [DoesNotReturn]
    public void Error(string message, Node n) {
        throw new Exception($"{message}\n\t at {n.source.line}:{n.source.col} in {n.source.file}");
    }

    public void Import(string path) {
        string secondarypath = string.Join(" ", path.Split(new char[] { '\\', '/' }));
        if (Imports.Contains(secondarypath)) 
            return;
        
        Tokens.AddRange(Lexer.Tokenize(File.ReadAllText(path), path));
        Imports.Add(secondarypath);
    }

    public Token Peek(int overhead = 1, params string[] type) {
        if(_position + overhead > Tokens.Count - 1)
            Error($"Unexpected end of input, expected: {String.Join(", ", type)}.");
        return Tokens[_position + overhead];
    }
    
    public Token Overhead(params string[] expect) {
        if(!expect.Contains(current().name()) && !expect.Contains("Any"))
            Error($"Unexpected token: {current().name()}, expected: {String.Join(", ", expect)}.");
        Token token = Tokens[_position];
        _position++;
        return token;
    }
    
    public Token Eat(params string[] expect) {
        if(!expect.Contains(current().type()) && !expect.Contains("Any"))
            Error($"Unexpected token: {current().name()}, expected: {String.Join(", ", expect)}.");
        Token token = Tokens[_position];
        _position++;
        return token;
    }

    public void Parse() {
        Program = new Node(current(), new Body());
        Daddy.Add(Program);
        while (parse_ongoing()) {
            Node node;
            
            switch (current().type()) {
                case "rule":
                    node = Rule.Resolve(this);
                    break;
                
                case "squiggly":
                    node = Squiggly.Resolve(this);
                    break;

                case "attribute":
                    node = Attribute.Resolve(this);
                    break;
                
                case "fn":
                    node = Function.Resolve(this);
                    break;
                
                case "let":
                    node = Variable.Resolve(this);
                    break;
                
                case "class":
                    node = Class.Resolve(this);
                    break;
                
                case "namespace":
                    node = Namespace.Resolve(this);
                    break;
                
                case "enum":
                    node = EnumStmt.Resolve(this);
                    break;
                
                case "hashtag":
                    node = Macro.Resolve(this);
                    break;

                default:
                    Error($"Unexpected token: {current().name()}.");
                    return;
            }
            
            Program.Add(node);
        }

        if (_position < Tokens.Count - 1) {
            _position++;
            Parse();
        }
    }
}