namespace lang; 

public class Rule {
    public class Event {
        public class EventValue { }
        public class EventArg { }

        private static readonly List<string> EventHeroes = new List<string>()
            { "all", "ramattra", "zenyatta", "ana" };

        // Event : ( event ) "(" (, arg * ) ")"
        public static Node ResolveEvent(Parser P, Token DefaultRuleToken) {
            if (P.current() == "arrow") {
                P.Eat("arrow");
                Node Event = new Node(P.Eat("event"), new Event());
                List<Node> Args = Rule.Event.ResolveArgs(P);

                Event.Add(Args);
                return Event;
            }

            return new Node(
                new Token(
                    "globalized", "event", DefaultRuleToken.file.FullName, DefaultRuleToken.line, DefaultRuleToken.col),
                new Event());
        }
        
        // Arg : Key ":" Value
        //       ^^^
        //    Identifier
        public static List<Node> ResolveArgs(Parser P) {
            List<Node> args = new List<Node>();
            P.Eat("lparan");

            while (P.current() != "rparan") {
                Node node = new Node(P.Eat("ident"), new EventArg());
                P.Eat("colon");
                Node value = ResolveValue(P, node);
                node.Add(value);
                args.Add(node);

                if (P.current() != "rparan")
                    P.Eat("comma");
            }

            P.Eat("rparan");

            return args;
        }

        public static Node ResolveValue(Parser P, Node key) {
            Token value;
            Node node;
            int actualvalue;
            switch (key.source.name()) {
                case "slot":
                    value = P.Eat("int", "ident");
                    if (value == "ident") {
                        node = new Node(value, new EventValue());
                        if(value.name().ToLower() != "all")
                            P.Error($"Event argument \"{key.source.name()}\" can only have values between 0 and 5, and \"all\"");

                        return node;
                    }

                    node = Expr.ResolveNumber(value);
                    // no need to convert to int but whatever
                    actualvalue = (int)node.Hold;
                    if(actualvalue < 0 || actualvalue > 5)
                        P.Error($"Event argument \"{key.source.name()}\" can only have values between 0 and 5, and \"all\".");
                    
                    return node;
                
                case "team":
                    value = P.Eat("int", "ident");
                    if (value == "ident") {
                        node = new Node(value, new EventValue());
                        if(value.name().ToLower() != "all")
                            P.Error($"Event argument \"{key.source.name()}\" can only have 3 values: 1, 2 and \"all\"");

                        return node;
                    }

                    node = Expr.ResolveNumber(value);
                    actualvalue = (int)node.Hold;
                    if(actualvalue != 1 && actualvalue != 2)
                        P.Error($"Event argument \"{key.source.name()}\" can only have 2 values: 1 and 2");
                    
                    return node;
                
                case "hero":
                    value = P.Eat("ident");
                    if(!EventHeroes.Contains(value.name().ToLower()))
                        P.Error($"Event argument \"{key.source.name()}\" can only have hero names as a value.");

                    return new Node(value, new EventValue());
            }
            
            P.Error("Unknown event value.");
            return null;
        } 
    }

    public class Condition {
        public class Conditions { }
        
        // Condition: | ( Expr )
        public static Node ResolveCondition(Parser P, Token DefaultRuleToken) {
            Node node = new Node(DefaultRuleToken, new Conditions());
            while (P.current() == "marker") {
                Node conditionHolder = new Node(P.Eat("marker"), new Condition());
                Node condition = Expr.Resolve(P);
                conditionHolder.Add(condition);
                
                node.Add(conditionHolder);
            }

            return node;
        }
    }
    
    // Rule : rule ( string ) ( -> ( event ) ) ( conditions ) block
    public static Node Resolve(Parser P) {
        Token rule = P.Eat("rule"), name = P.Eat("string");
        Node RuleNode = new Node(rule, new Rule()),
            NameNode = Expr.ResolveLiteral(name),
            EventNode = Event.ResolveEvent(P, rule),
            ConditionNode = Condition.ResolveCondition(P, rule);

        Node Block = lang.Block.Resolve(P);
        RuleNode.Add(NameNode, EventNode, ConditionNode, Block);

        return RuleNode;
    }
}
