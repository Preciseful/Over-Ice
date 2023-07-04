using System.Text.RegularExpressions;

namespace lang;

public class Token {
    public int col, line;
    public string _name, _type;
    public FileInfo file;

    public string name() => _name;
    public string type() => _type;

    public Token(string _name, string _type, string file, int line, int col) {
        this._name = _name;
        this._type = _type;
        this.file = new FileInfo(file);
        this.line = line;
        this.col = col;
    }
    
    public static bool operator ==(Token a, string b) => a.type() == b;
    public static bool operator !=(Token a, string b) => a.type() != b;
    public override bool Equals(object? obj) => this == (string)(obj ?? throw new Exception("Invalid equality."));
    public override int GetHashCode() => type().GetHashCode();
}

public class Lexer {
    public const string pattern = @"(([0-9]+\.[0-9]+)|(//.*\n)|(?:->)|(?:=>)|([&+\-/%*=|])\4|([+\-/%!*^><]=)|[.;{}()\[\]:=&|#><~]|(/\*[\s\S]*\*/)|([+-/%!*])|""[^""\\]*(?:\\.[^""\\]*)*""|[a-zA-Z0-9_]+)|\n| ";

    public static readonly Dictionary<string, string> KEYWORDS = new Dictionary<string, string>() {
        { "const",        "attribute"     },
        { "static",       "attribute"     },
        { "public",       "attribute"     },
        { "private",      "attribute"     },
        { "global",       "attribute"     },
        { "local",        "attribute"     },
        
        { "int",          "type"          },
        { "string",       "type"          },
        { "void",         "type"          },
        { "bool",         "type"          },
        { "Vector",       "type"          },
        { "Player",       "type"          },
        { "Entity",       "type"          },
        { "Text",         "type"          },
        
        { "true",         "bool"          },
        { "false",        "bool"          },

        { "rule",         "rule"          },
        { "localized",    "event"         },
        { "globalized",   "event"         },
        
        { "+",            "operator"      },
        { "-",            "operator"      },
        { "/",            "operator"      },
        { "*",            "operator"      },
        { "%",            "operator"      },
        { "^",            "operator"      },
         
        { "=",             "assign"        },
        { "+=",            "assign"        },
        { "-=",            "assign"        },
        { "/=",            "assign"        },
        { "*=",            "assign"        },
        { "%=",            "assign"        },
        { "^=",            "assign"        },

        { "==",           "compare"       },
        { "!=",           "compare"       },
        
        { ";",            "semicolon"     },
        { ":",            "colon"         },
        { "::",           "double_colon"  },
        { "++",           "increment"     },
        { "--",           "decrement"     },
        { "&&",           "and"           },
        { "and",          "and"           },
        { "||",           "or"            },
        { "or",           "or"            },
        { "!",            "not"           },
        { "not",          "not"           },
        { "(",            "lparan"        },
        { ")",            "rparan"        },
        { "{",            "lbracket"      },
        { "}",            "rbracket"      },
        { "[",            "larray"        }, 
        { "]",            "rarray"        },
        { "|",            "marker"        },
        { ",",            "comma"         },
        { ".",            "dot"           },
        { "<",            "smaller"       },
        { ">",            "bigger"        },
        { "->",           "arrow"         },
        { "=>",           "double_arrow"  },
        { "<=",           "smaller_eq"    },
        { ">=",           "bigger_eq"     },
        { "#",            "hashtag"       },

        { "return",       "return"        },
        { "for",          "for"           },
        { "while",        "while"         },
        { "namespace",    "namespace"     },
        { "class",        "class"         },
        { "enum",         "enum"          },
        { "new",          "new"           },
        { "template",     "template"      },
        { "unoptimize",   "unoptimize"    },
        { "fn",           "fn"            },
        { "let",          "let"           },
        
        { "if",           "if"            },
        { "elif",         "elif"          },
        { "else",         "else"          },
    };
    
    static List<string> Lex(string input) {
        input += "\n";
        List<string> matches = new List<string>();
        foreach (Match match in Regex.Matches(input, pattern)) 
            matches.Add(match.Value);

        return matches;
    }

    static Token Match(string input, int line, int col, string file) {
        Token token;

        if (KEYWORDS.TryGetValue(input, out var value))
            token = new Token(input, value, file, line, col);
        // Exceptions
        else if (input.StartsWith("\""))
            token = new Token(input, "string", file, line, col);
        else if(input.StartsWith("~"))
            token = new Token(input, "squiggly", file, line, col);
        else if (int.TryParse(input, out _)) {
            if(input.Length > 10)
                token = new Token(input, "long", file, line, col);
            else
                token = new Token(input, "int", file, line, col);
        }
        else if (float.TryParse(input, out _))
            token = new Token(input, "float", file, line, col);
        else
            token = new Token(input, "ident", file, line, col);

        return token;
    }
    
    public static List<Token> Tokenize(string input, string file) {
        List<string> lexers = Lex(input);
        List<Token> tokens = new List<Token>();
        int line = 1, col = 0;

        for (int i = 0; i < lexers.Count(); i++) {
            string m = lexers[i];

            if (col >= 1) 
                col += lexers[i - 1].Length;
            else
                col = 1;
            
            if(m.StartsWith("/*") && m.EndsWith("*/"))
                continue;

            if (m.StartsWith("//")) {
                line++;
                col = 0;
                continue;
            }


            if (m == "\n") {
                line++;
                col = 0;
                continue;
            }

            if (m == " ") 
                continue;
            

            if (tokens.Count > 0 && tokens.Last().name() == "else" && m == "if") {
                tokens[tokens.Count - 1] = Match("elif", line, col, file);
                continue;
            }
            
            if (tokens.Count > 0 && tokens.Last()=="colon" && m == ":") {
                tokens[tokens.Count - 1] = Match("::", line, col, file);
                continue;
            }
            
            tokens.Add(Match(m, line, col, file));

            if (tokens.Last() == "comment") {
                line++;
                col = 0;
            }
        }
        
        tokens.Add(new Token("eof", "eof", file, line, col));
        return tokens;
    }
}