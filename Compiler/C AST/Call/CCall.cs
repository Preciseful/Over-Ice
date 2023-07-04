namespace lang; 
using Newtonsoft.Json.Linq;

public class CCall : Tree, Leaf {
    private List<Leaf> _leaves = new List<Leaf>();

    private string _name, _type;
    private CFunction _reference;
    
    public List<Leaf> leaves() => _leaves;
    public Dictionary<int, Leaf> indexes = new Dictionary<int, Leaf>();
    public Tree? tree {get; set;}
    public string name() => _name;
    public string type() => _type;
    public CFunction reference() => _reference;

    public CCall(string name, CFunction refer, Tree? tree = null) {
        this._name = name;
        this.tree = tree;
        this._reference = refer;
        this._type = reference().type();
        this.tree?.leaves().Add(this);
    }

    public void Refresh() {
        if(this.reference().name()[0] != '%')
            return;
        List<Leaf> newLeaves = new List<Leaf>();
        List<CAccessParameter> accesses = this.leaves()
            .FindAll(l => l as CAccessParameter != null)
            .ConvertAll(l => (CAccessParameter)l);

        int offset = -1;
        foreach (var index in indexes) {
            if (index.Key > offset)
                offset = index.Key;
        }
        
        offset = int.Max(offset + 1, this.leaves().Count);
        for (int i = 0; i < offset; i++) {
            CAccessParameter? access = accesses.Find(a => a.index == i);
            if(access != null)
                newLeaves.Add(access.Expr);
            else if (indexes.TryGetValue(i, out var leaf)) 
                newLeaves.Add(leaf);
            else
                newLeaves.Add(new CDEFAULT());
        }

        _leaves = newLeaves;
    }

    public static CCall Unpack(Node cnode, Tree tree) {
        string name = cnode.source.name();
        CCall call = new CCall(name, GetRef(cnode, name, tree), tree);

        foreach (Node node in cnode.Children) {
            if (node.Hold as Call.AccessParameter != null) {
                if(call.leaves().Exists(l => l as CAccessParameter != null && l.name() == node.source.name()))
                    Compiler.Error("Already declared parameter.", node);
                
                Leaf child = CExpr.Convert(node.Consume(0)!, call);
                call.leaves().Remove(child);
                CAccessParameter access = new CAccessParameter(node.source.name(), child.type(), child, call);
                access.Expr = child;
            }
            else    
                CExpr.Convert(node, call);
        }
        

        if (call.reference().name()[0] == '%') {
            if (Compiler.Values.TryGetValue(name, out var value)) {
                if(tree as CRule != null || tree as CFunction != null)
                    Compiler.Error("Invalid line.", cnode);

                call = VerifyWorkshopParams(call, cnode, value.ToObject<JObject>()!, tree);
                call.Refresh();
                return call;
            }

            call = VerifyWorkshopParams(call, cnode, Compiler.Actions[name]!.ToObject<JObject>()!, tree);
            call.Refresh();
            return call;
        }
        
        //todo optimize
        List<CFunction> allpossibleReferences =
            tree.FindAll(l => l as CFunction != null && l.name() == name)
                .ConvertAll(l => (CFunction)l);
        // we check count to be 1 and not 0 because the current reference is already included
        int necessaryParams = call.reference().parameters.FindAll(v => v.initialvalue == null).Count;
        
        if (allpossibleReferences.Count == 1) {
            if (necessaryParams > call.leaves().Count)
                Compiler.Error($"Lack of arguments.", cnode);
            if (call.leaves().Count > call.reference().parameters.Count)
                Compiler.Error($"Overloaded arguments.", cnode);
        }

        if(FindPossibleReference(call, allpossibleReferences, cnode) == null)
            Compiler.Error($"Unmatching parameter types.", cnode);
        
        call.Refresh();
        return call;
    }

    private static CFunction GetRef(Node cnode, string name, Tree tree) {
        CFunction? function = tree.Find(l => l as CFunction != null && l.name() == name) as CFunction;
        if (function != null)
            return function;
        if(Compiler.Actions.TryGetValue(name, out var action))
            return new CFunction($"%{name}", action["type"]!.ToString());
        if (Compiler.Values.TryGetValue(name, out var value)) 
            return new CFunction($"%{name}", value["type"]!.ToString());
        
        Compiler.Error("Unknown identifier.", cnode);
        return null;
    }

    private static CCall VerifyWorkshopParams(CCall call, Node cnode, JObject action, Tree tree) {
        string lhv = "";

        if (action["type"]!.ToString() == "define") {
            call.HandleDefine(cnode);
            return call;
        }
            
        if (action["lhv-only"] != null) {
            lhv = action["lhv-only"]!.ToString();
            // perhaps they meant to invoke a similar function that doesn't exist
            if (tree as CInvokeExpr == null)
                Compiler.Error("Unknown identifier.", cnode);
            
            if (lhv == "define") {
                call.HandleDefine(cnode);
                return call;
            }
            
            bool matchType = CExpr.VerifyTypes(action["args"]![lhv]!["type"]!.ToString(), tree.leaves()[0].type());
            if (!matchType)
                Compiler.Error("Unknown identifier.", cnode);
        }

        if (action["args"] == null)
            return call;
        
        JObject parameters = action["args"]!.ToObject<JObject>()!;
        List<string> filledParameters = new List<string>();
        List<CAccessParameter> accesses = call.leaves()
            .FindAll(l => l as CAccessParameter != null)
            .ConvertAll(l => (CAccessParameter)l);
        
        int index = 0;
        foreach (var parameter in parameters) {
            if (parameter.Key == lhv) 
                continue;

            if (index > call.leaves().Count - 1) {
                // everything from here on out is gonna be optional so
                if (parameter.Value!["default"] != null)
                    return call;
                Compiler.Error("Missing parameter.", cnode);
            }
            
            Node node = cnode.Children[index];
            Leaf arg = accesses.Find(a => a.name() == parameter.Key) ?? call.leaves()[index];
            if (arg as CAccessParameter != null) {
                if (arg.name() != parameter.Key) {
                    if (parameter.Value!["default"] == null)
                        Compiler.Error("Missing parameter.", cnode);
                    continue;
                }

                CAccessParameter accessParameter = (CAccessParameter)arg;
                node = cnode.Children.Find(c =>
                    c.Hold as Call.AccessParameter != null && c.source.name() == arg.name())!;

                accessParameter.index = (int)parameter.Value!["index"]!;
                index--;
            }
            
            // use try bc "ToObject" will crash everything, even if i check for null
            try {
                JArray typeArray = parameter.Value!["type"]!.ToObject<JArray>()!;
                if (!CExpr.VerifyTypes(arg.type(), typeArray))
                    Compiler.Error("Unmatching types.", node);
                if(filledParameters.Contains(parameter.Key))
                    Compiler.Error("Parameter is already declared.", node);
                
                filledParameters.Add(parameter.Key);
                index++;
            }
            catch {
                string argtype = parameter.Value!["type"]!.ToString();
                if (!CExpr.VerifyTypes(arg.type(), argtype))
                    Compiler.Error("Unmatching types.", node);
                if(filledParameters.Contains(parameter.Key))
                    Compiler.Error("Parameter is already declared.", node);
                filledParameters.Add(parameter.Key);
                index++;
            }
            
            if(arg as CAccessParameter != null)
                call.indexes.Add(((CAccessParameter)arg).index, ((CAccessParameter)arg).Expr);
            else if(parameter.Value["index"] != null)
                call.indexes.Add((int)parameter.Value["index"]!, arg);
            else 
                call.indexes.Add(index, arg);
        }

        return call;
    }
    

    private static CFunction? FindPossibleReference(CCall call, List<CFunction> allPossibleReferences, Node cnode) {
        Node? LastParameterError = null;
        List<CFunction> Founds = new List<CFunction>();
        
        foreach (CFunction function in allPossibleReferences) {
            int necessaryParams = function.parameters.FindAll(v => v.initialvalue == null).Count;
            if(!(necessaryParams <= call.leaves().Count && call.leaves().Count <= function.parameters.Count))
                continue;

            bool found = true;
            List<Leaf> parametersFilled = new List<Leaf>();

            for (int i = 0; i < call.leaves().Count; i++) {
                Leaf callchild = call.leaves()[i], parameter = function.parameters[i];
                
                if (callchild as CAccessParameter != null) {
                    CVariable? accessedParam = function.parameters.Find(p => p.name() == callchild.name());
                    if (accessedParam == null) {
                        LastParameterError = cnode.Children[i];
                        break;
                    }
                    
                    if(parametersFilled.Contains(accessedParam))
                        Compiler.Error("Parameter already declared.", cnode.Children[i]);
                    parametersFilled.Add(accessedParam);

                    if (!CExpr.VerifyTypes(accessedParam, callchild)) {
                        found = false;
                        break;
                    }

                    ((CAccessParameter)callchild).index = function.parameters.IndexOf(accessedParam);
                    continue;
                }
                
                else if (!CExpr.VerifyTypes(parameter, callchild)) {
                    found = false;
                    break;
                }
                
                parametersFilled.Add(parameter);
            }

            if (found) 
                Founds.Add(function);
        }
        
        
        if(Founds.Count > 1)
            Compiler.Error("Ambigous function call.", cnode);
        if (Founds.Count == 1) {
            call._reference = Founds[0];
            return Founds[0];
        }

        if(LastParameterError != null)
            Compiler.Error("Invalid parameter.", LastParameterError);

        return null;
    }

    private void HandleDefine(Node node) {
        switch (this.name()) {
            case "append":
                HandleGeneric(node, "append to");
                break;
            case "indexOf":
                HandleGeneric(node, "index of from");
                break;
            case "remove":
                HandleGeneric(node, "remove from");
                break;
            case "removeAt":
                HandleRemoveAt(node);
                break;
            case "sort":
                HandleArrayOperations(node, "%sort", "sort");
                break;
            case "map":
                HandleArrayOperations(node, "%map", "map");
                break;
            case "once":
                HandleOnce();
                break;

            default:
                throw new Exception("Compiler error: Action found with <define> as type, but no handler for it.");
        }
    }

    private void HandleGeneric(Node node, string message) {
        Leaf lhv = this.tree!.leaves()[0];
        if (lhv as CVariable == null && (lhv as CInvokeExpr)?.LatestChild as CVariable == null)
            Compiler.Error($"Cannot {message} non-variables.", node);
        
        if (!lhv.type().Contains("array"))
            Compiler.Error($"Cannot {message} non-array variables.", node);
    
        string expectedType = lhv.type().Remove(0, 5).Trim();
        if (this.leaves()[0] as CAccessParameter != null) {
            if(this.leaves()[0].name() != "value")
                Compiler.Error("Invalid parameter.", node.Get(0)!);
            this.leaves()[0] = ((CAccessParameter)this.leaves()[0]).Expr;
            this.leaves()[0].tree = this;
        }

        if (!CExpr.VerifyTypes(this.leaves()[0].type(), expectedType))
            Compiler.Error("Unmatching types.", node);
        
        this.indexes.Add(0, this.leaves()[0]);
    }

    private void HandleRemoveAt(Node node) {
        Leaf lhv = this.tree!.leaves()[0];
        if (lhv as CVariable == null && (lhv as CInvokeExpr)?.LatestChild as CVariable == null)
            Compiler.Error("Cannot remove from non-variables.", node);
        
        if (!lhv.type().Contains("array"))
            Compiler.Error("Cannot remove from non-array variables.", node);
        
        if (this.leaves()[0] as CAccessParameter != null) {
            if(this.leaves()[0].name() != "index")
                Compiler.Error("Invalid parameter.", node.Get(0)!);
            this.leaves()[0] = ((CAccessParameter)this.leaves()[0]).Expr;
            this.leaves()[0].tree = this;
        }

        if (this.leaves()[0].type() != "int")
            Compiler.Error("Invalid type.", node);
        
        this.indexes.Add(0, this.leaves()[0]);
    }

    private void HandleOnce() {
        this._type = this.leaves()[0].type();
        this.indexes.Add(0, this.leaves()[0]);
    }
    
    private void HandleArrayOperations(Node node, string call, string message) {
        Leaf lhv = this.tree!.leaves()[0];
        if (!lhv.type().Contains("array"))
            Compiler.Error($"Cannot {message} non-arrays.", node);

        CLambda lambda = CLambda.Unpack(node.Get(0)!, lhv.type().Remove(0, 5).Trim(), call, this);
        this.indexes.Add(0, lambda);
        this._type = lambda.type();
    }
}
