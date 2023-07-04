using System.Diagnostics.Contracts;
using System.Globalization;
using Newtonsoft.Json.Linq;
using TextCopy;

namespace lang;

public class CWorkshop {
    private CProgram C;
    private int ident;
    private string result = "";
    private List<string> pendresult = new List<string>();
    private bool isInUnoptimize;
    private readonly char[] digits = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

    [Pure]
    private string tab() => new string('\t', ident);

    [Pure]
    private string newline(int count) => new string('\n', count);

    private void PendWrite(string message) => pendresult.Add(message);

    private void Write(string message, int newlines = 1, bool usepend = true) {
        if (message.StartsWith("-all"))
            return;

        if (!message.StartsWith("-result"))
            result += $"{newline(newlines)}{tab()}{message}";
    
        if (usepend && pendresult.Count > 0) {
            result += $"{newline(newlines)}{tab()}{string.Join($"{newline(newlines)}{tab()}", pendresult)}";
            pendresult.Clear();
        }
    }

    private static JObject Translations = Compiler.Workshop["translations"]!.ToObject<JObject>()!;
    private static Dictionary<CVariable, string> Variables = new Dictionary<CVariable, string>();

    public CWorkshop(CProgram C) {
        this.C = C;
    }

    public enum ConstantExprType {
        GetFalse,
        GetTrue,
        False
    }

    private static readonly HashSet<string> ComparisonSet = new HashSet<string>()
        { "==", "!=", "<", "<=", ">=", ">" };

    public void Start() {
        foreach (Leaf leaf in C.leaves()) {
            CBody body = (CBody)leaf;
            List<CRule> cRules = body.leaves().FindAll(l => l as CRule != null).ConvertAll(l => (CRule)l);
            foreach (CRule rule in cRules) {
                if(rule.leaves().Count == 0)
                    continue;
                TranslateRule(rule);
            }
        }
        
        ClipboardService.SetText(WriteVariables() + result);
    }

    private void StartBlock(bool usepend = true) {
        Write("{", 1, usepend);
        ident++;
    }

    private void EndBlock(bool usepend = true) {
        ident--;
        Write("}", 1, usepend);
    }
    
    private void TranslateRule(CRule rule) {
        Write($"rule({rule.name()})");
        StartBlock();

        Write("Event");
        StartBlock();
        Write(Compiler.Events[rule.Event.name] + ";");
        if (rule.Event.name != "globalized") {
            Write($"{(rule.Event.team == "All" ? rule.Event.team : $"Team {rule.Event.team}")};");
            Write($"{(
                rule.Event.slot == ""
                    ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(rule.Event.hero!.ToLower())
                    : $"Slot {rule.Event.slot}"
            )};");
        }

        EndBlock();

        if (rule.Conditions.Count > 0 && rule.Conditions.FindAll(c => TranslateCondition(c)).Count > 0) {
            Write("Conditions", 2, false);
            StartBlock(false);
            Write("-result");
            EndBlock(false);
        }

        pendresult.Clear();

        if (rule.leaves().Count > 0) {
            Write("Actions", 2);
            StartBlock();
            rule.leaves().ForEach(l => TranslateAction(l));
            EndBlock();
        }

        EndBlock();
        Write("\n");
    }

    private string WriteVariables() {
        string varresult = "";
        List<KeyValuePair<CVariable, string>> locals = Variables.Where(c => c.Key.accessibility.name == "local").ToList(),
            globals = Variables.Where(c => c.Key.accessibility.name == "global").ToList();
        
        List<CVariable> localHolds = locals.FindAll(l => l.Key.initialvalue != null && l.Key.initTree as CBody != null).Select(l => l.Key).ToList(),
            globalHolds = globals.FindAll(l => l.Key.initialvalue != null && l.Key.initTree as CBody != null).Select(l => l.Key).ToList();
        
        varresult += "variables \n{\n";
        int index = 0;
        ident++;
        varresult += $"{tab()}global:\n";
        ident++;
        varresult += $"{tab()}{index}: DEW_RESULT\n";
        index++;
        
        if (locals.Count > 0 || globals.Count > 0) {
            globals = Variables.Where(c => c.Key.accessibility.name == "global").ToList();
            if (globals.Count() > 0) {
                foreach (KeyValuePair<CVariable, string> variable in globals) {
                    varresult += $"{tab()}{index}: {variable.Value}\n";
                    index++;
                }
            }
            
            ident--;
            
            if (locals.Count > 0) {
                varresult += $"{tab()}player:\n";
                ident++;
                index = 0;
                foreach (KeyValuePair<CVariable, string> variable in locals) {
                    varresult += $"{tab()}{index}: {variable.Value}\n";
                    index++;
                }
            }

            ident = 0;
        }
        
        varresult += "}\n\n";
        
        ident = 2;
        if (globalHolds.Count > 0) {
            varresult += "rule(\"Init Global Variables\")" +
                         "\n{\n\tevent\n\t{" +
                         "\n\t\tOngoing - Global;" +
                         "\n\t}\n\t" +
                         "\n\tactions\n\t{";
            foreach (CVariable variable in globalHolds) 
                varresult += $"\n{tab()}{VisitVar(variable)} = {VisitExpr(variable.initialvalue!)};";

            varresult += "\n\t}\n}\n";
        }
        
        if (localHolds.Count > 0) {
            varresult += "rule(\"Init Local Variables\")" +
                         "\n{\n\tEvent\n\t{" +
                         "\n\t\tOngoing - Each Player;" +
                         "\n\t\tAll;" +
                         "\n\t\tAll;" +
                         "\n\t}\n\t" +
                         "\n\tActions\n\t{";
            foreach (CVariable variable in localHolds) 
                varresult += $"\n{tab()}{VisitVar(variable)} = {VisitExpr(variable.initialvalue!)};";
            
            varresult += "\n\t}\n}\n";
        }

        return varresult;
    }

    private bool TranslateCondition(CCondition condition) {
        string expr = VisitExpr(condition.leaves()[0]);
        if (expr == "True")
            return false;
        // encountered a rule that will never run, make it disabled
        if (expr == "False") {
            int index = result.LastIndexOf("rule", StringComparison.Ordinal);
            result = result.Insert(index, "disabled ");          
            
            return false;
        }

        PendWrite($"{(ComparisonSet.Contains(condition.leaves()[0].name()) ? expr + ";" : $"{expr} == True;")}");
        return true;
    }

    private void TranslateAction(Leaf leaf) {
        Write(VisitLine(leaf));
    }

    private string VisitExpr(Leaf leaf) {
        if (leaf as CBinaryExpr != null)
            return VisitBinaryExpr((CBinaryExpr)leaf);
        if (leaf as CUnaryExpr != null)
            return VisitUnaryExpr((CUnaryExpr)leaf, true);
        if (leaf as CInvokeExpr != null)
            return VisitInvokeExpr((CInvokeExpr)leaf);
        if (leaf as CEnum.Invoke != null)
            return VisitEnumInvokeExpr((CEnum.Invoke)leaf);
        if (leaf as CCall != null)
            return VisitCall((CCall)leaf, true);
        if (leaf as CVariable != null)
            return VisitVar((CVariable)leaf);
        if (leaf as CLambda != null)
            return VisitExpr(((CLambda)leaf).leaves()[1]);
        if (leaf as LambdaVar != null)
            return "Current Array Element";
        if (leaf as CArrayIndex != null)
            return VisitIndex((CArrayIndex)leaf);
        if (leaf as CInt != null || leaf as CFloat != null)
            return $"{leaf.name()}";
        if (leaf as CArray != null) {
            CArray array = (CArray)leaf;
            if (array.leaves().Count == 0)
                return "Empty Array";
            string temp = "Array(";
            foreach (Leaf child in array.leaves())
                temp += VisitExpr(child) + ", ";
            return temp.Remove(temp.Length - 2) + ")";
        }

        if (leaf as CString != null)
            return $"Custom String({leaf.name()})";
        if (leaf as CBool != null)
            return Translations[leaf.name()]!.ToString();

        throw new Exception($"Unhandled expression leaf. {leaf.GetType()}");
    }

    private string VisitLine(Leaf leaf) {
        if (leaf as CVariable != null) {
            CVariable var = (CVariable)leaf;
            return $"{VisitVar(var)} = {(var.initialvalue != null ? VisitExpr(var.initialvalue) : "null")};";
        }

        if (leaf as CFunction != null)
            return $"// Declared function: {leaf.name()}";
        if (leaf as CCall != null)
            return VisitCall((CCall)leaf, false) + ";";
        if (leaf as CUnoptimize != null)
            return VisitUnoptimize((CUnoptimize)leaf);
        if (leaf as CUnaryExpr != null)
            return VisitUnaryExpr((CUnaryExpr)leaf, false) + ";";
        if (leaf as CBinaryExpr != null)
            return VisitBinaryExpr((CBinaryExpr)leaf) + ";";
        if (leaf as CInvokeExpr != null)
            return VisitInvokeExpr((CInvokeExpr)leaf) + ";";
        if (leaf as CAssign != null)
            return VisitAssign((CAssign)leaf);
        if (leaf as CReturn != null)
            return VisitReturn((CReturn)leaf);
        if (leaf as CFor != null)
            return VisitFor((CFor)leaf);
        if (leaf as CWhile != null)
            return VisitWhile((CWhile)leaf);
        if (leaf as CIf != null)
            return VisitIf((CIf)leaf);

        throw new Exception("Unhandled line leaf.");
    }

    private bool Similar(Leaf lhv, Leaf rhv) {
        if (lhv.name() == rhv.name() && lhv.GetType() == rhv.GetType()) {
            if (lhv as CCall != null && rhv as CCall != null &&
                ((CCall)lhv).reference() != ((CCall)rhv).reference())
                return false;

            return true;
        }

        return false;
    }

    private bool isConstant(Leaf value) =>
        value as CInt != null || value as CBool != null || value as CString != null || value as CEnum.Invoke != null;

    private ConstantExprType isConstantExpression(Leaf lhv, string op, Leaf rhv) {
        if (op == "==") {
            if (Similar(lhv, rhv))
                return ConstantExprType.GetTrue;
            string lhvexpr = VisitExpr(lhv), rhvexpr = VisitExpr(rhv);
            if (lhvexpr == rhvexpr)
                return ConstantExprType.GetTrue;
            
            if (!isConstant(lhv) || !isConstant(rhv))
                return ConstantExprType.False;

            return ConstantExprType.GetFalse;
        }
        
        else if (op == "!=") {
            if (!Similar(lhv, rhv))
                return ConstantExprType.GetTrue;
            string lhvexpr = VisitExpr(lhv), rhvexpr = VisitExpr(rhv);

            if (!isConstant(lhv) || !isConstant(rhv))
                return ConstantExprType.False;
            
            if (lhvexpr != rhvexpr)
                return ConstantExprType.GetTrue;
            return ConstantExprType.GetFalse;
        }

        return ConstantExprType.False;
    }

    private string VisitUnoptimize(CUnoptimize unoptimize) {
        List<string> temp = new List<string>();
        isInUnoptimize = true;
        foreach (Leaf line in unoptimize.leaves())
            temp.Add(VisitLine(line));

        isInUnoptimize = false;
        return string.Join($"\n{tab()}", temp);
    }

    private string VisitBinaryExpr(CBinaryExpr expr) {
        if (expr.tree as CCondition != null && ComparisonSet.Contains(expr.name())) {
            Leaf lhv = expr.leaves()[0], rhv = expr.leaves()[1];
            ConstantExprType type = isConstantExpression(lhv, expr.name(), rhv);
            if (type == ConstantExprType.GetTrue)
                return "True";
            else if (type == ConstantExprType.GetFalse)
                return "False";
            
            return $"{VisitExpr(lhv)} {expr.name()} {VisitExpr(rhv)}";
        }

        if (ComparisonSet.Contains(expr.name())) {
            Leaf lhv = expr.leaves()[0], rhv = expr.leaves()[1];
            ConstantExprType type = isConstantExpression(lhv, expr.name(), rhv);
            if (type == ConstantExprType.GetTrue)
                return "True";
            else if (type == ConstantExprType.GetFalse)
                return "False";
            return $"Compare({VisitExpr(lhv)}, {expr.name()}, {VisitExpr(rhv)})";
        }

        return $"{Translations[expr.name()]}({VisitExpr(expr.leaves()[0])}, {VisitExpr(expr.leaves()[1])})";
    }

    private string VisitUnaryExpr(CUnaryExpr expr, bool use) {
        if (expr.name() == "++" || expr.name() == "--") {
            if (use) {
                Write($"{VisitExpr(expr.leaves()[0])} {expr.name()[0]}= 1;");
                return $"{VisitExpr(expr.leaves()[0])}";
            }
            
            return $"{VisitExpr(expr.leaves()[0])} {expr.name()[0]}= 1";
        }
        
        return $"{Translations[expr.name()]}({VisitExpr(expr.leaves()[0])})";
    }

    private string VisitInvokeExpr(CInvokeExpr expr) {
        if (expr.leaves()[1] as CVariable != null)
            return VisitVar((CVariable)expr.leaves()[1], expr.leaves()[0]);
        else if ((expr.leaves()[1] as CCall)?.reference().name()[0] == '%')
            return $"{VisitWorkshopCall((CCall)expr.leaves()[1], expr.leaves()[0], true)}";
        return $"{VisitExpr(expr.leaves()[0])}.{VisitExpr(expr.leaves()[1])}";
    }

    private string VisitEnumInvokeExpr(CEnum.Invoke expr) {
        CEnum.Value value = expr.value;
        if (value.isWorkshop) {
            JToken en = Compiler.Enums[expr.type()]!;
            return en["translationType"]?.ToString() == "call" 
                ? $"{en["translation"]}({en["values"]![value.name()]})" 
                : en["values"]![value.name()]!.ToString();
        }
        
        return $"{value.index}";
    }

    private string VisitAssign(CAssign assign) {
        return $"{VisitExpr(assign.leaves()[0])} {assign.name()} {VisitExpr(assign.leaves()[1])};";
    }

    private string VisitVar(CVariable variable, Leaf? Value = null) {
        if (variable.attributes.Exists(a => a.name == "*")) 
            return $"{VisitExpr(variable.initialvalue!)}";        
        
        string name;
        if (Variables.TryGetValue(variable, out var value))
            name = value;
        else {
            int count = Variables.Values.Where(v => variable.name() == v.TrimEnd(digits)).Count();
            Variables.Add(variable, name = variable.name() + (count > 0 ? count : ""));
        }

        if(variable.accessibility.name == "global")
            return $"{Translations[variable.accessibility.name]}.{name}";
        if (Value == null)
            return $"Event Player.{name}";
        return $"{VisitExpr(Value)}.{name}";
    }

    private string VisitFunction(CFunction function, CCall? call = null) {
        string temp = "";
        for (int i = 0; i < function.parameters.Count; i++) {
            CVariable parameter = function.parameters[i];
            if (call != null && i < call.leaves().Count) {
                if (call.leaves()[i] as CAccessParameter != null) {
                    CAccessParameter access = (CAccessParameter)call.leaves()[i];
                    if (function.parameters.Find(p => p.name() == access.name())!.attributes
                        .Exists(a => a.name == "*")) {
                        function.parameters.Find(p => p.name() == access.name())!.initialvalue = access.Expr;
                        continue;
                    }

                    temp += $"{VisitVar(function.parameters.Find(p => p.name() == access.name())!)} = {VisitExpr(access.Expr)};\n";
                }
                else {
                    if (parameter.attributes.Exists(a => a.name == "*")) {
                        parameter.initialvalue = call.leaves()[i];
                        continue;
                    }

                    temp += $"{VisitVar(parameter)} = {VisitExpr(call.leaves()[i])};\n";
                }
            }
            else if (parameter.initialvalue != null) {
                if(parameter.attributes.Exists(a => a.name == "*")) 
                    continue;
                
                temp += $"{VisitVar(parameter)} = {VisitExpr(parameter.initialvalue)};\n";
            }
        }

        if (function.DirectReturn)
            return VisitExpr(function.leaves()[0]);
        foreach (Leaf leaf in function.leaves()) 
            temp += VisitLine(leaf) + "\n";
        
        if(temp.Length > 0)
            return temp.Remove(temp.Length - 1).Replace("\n", $"\n{tab()}");
        return "";
    }

    private string VisitCall(CCall call, bool use, Leaf? lhvLeaf = null) {
        if (call.reference().name()[0] == '%')
            return VisitWorkshopCall(call, lhvLeaf, use);
        
        if(call.tree as CCondition != null)
            throw new NotImplementedException();

        if (call.reference().attributes.Exists(c => c.name == "~pure") && !use)
            return "-all";
        if (call.reference().DirectReturn) {
            if (!use)
                return "-all";
            
            return VisitFunction(call.reference(), call);
        }
        
        Write(VisitFunction(call.reference(), call));
        // todo: use extended variables like ostw
        if(use)
            return "Global.DEW_RESULT";
        return "-all";
    }

    private string VisitWorkshopCall(CCall call, Leaf? lhvLeaf, bool use) {
        int index = 0;
        JToken? token;
        Compiler.Values.TryGetValue(call.name(), out token);
        if (token == null)
            token = Compiler.Actions[call.name()];

        JObject action = token!.ToObject<JObject>()!;
        
        switch (call.name()) {
            case "append":
                return VisitWorkshopOperation(call, lhvLeaf!, "Append To Array");
            case "indexOf":
                return $"Index Of Array Value({VisitExpr(lhvLeaf!)}, {VisitExpr(call.leaves()[0])})";
            case "remove":
                return VisitWorkshopOperation(call, lhvLeaf!, "Remove From Array By Value");
            case "removeAt":
                return VisitWorkshopOperation(call, lhvLeaf!, "Remove From Array By Index");
            case "sort":
                return VisitWorkshopArrayOperation(call, lhvLeaf!, "Sorted Array");
            case "map":
                return VisitWorkshopArrayOperation(call, lhvLeaf!, "Mapped Array");
            case "once":
                return $"Evaluate Once({VisitExpr(call.leaves()[0])})";
        }
        
        string temp = $"{action["translation"]!}";
        if (action["args"] == null)
            return temp;

        temp += "(";
        JObject args = action["args"]!.ToObject<JObject>()!;
        if (lhvLeaf != null)
            temp += VisitExpr(lhvLeaf) + ", ";
        
        foreach (var arg in args) {
            if (index < call.leaves().Count) {
                if(call.leaves()[index] as CDEFAULT != null)
                    temp += arg.Value!["default"] + ", ";
                else
                    temp += VisitExpr(call.leaves()[index]) + ", ";
            }
            else if(arg.Value!["default"] != null)
                temp += arg.Value!["default"] + ", ";
            index++;
        }
        
        return temp.Remove(temp.Length - 2) + ")";
    }

    private string VisitWorkshopOperation(CCall call, Leaf lhv, string op) {
        CVariable variable = (lhv as CInvokeExpr)?.LatestChild as CVariable ?? (CVariable)lhv;

        if (!Variables.ContainsKey(variable))
            Variables.Add(variable, lhv.name());
        
        return variable.accessibility.name == "global"
                ? $"Modify Global Variable({variable.name()}, {op}, {VisitExpr(call.leaves()[0])})"
                : $"Modify Player Variable({(lhv as CInvokeExpr != null ? VisitExpr((lhv as CInvokeExpr)!.leaves()[0]) : "Event Player")}, {variable.name()}, {op}, {VisitExpr(call.leaves()[0])})";
    }
    
    private string VisitWorkshopArrayOperation(CCall call, Leaf lhv, string name) {
        return $"{name}({VisitExpr(lhv)}, {VisitExpr(call.leaves()[0])})";
    }

    private string VisitIndex(CArrayIndex index) {
        return $"{VisitExpr(index.access)}[{VisitExpr(index.index)}]";
    }

    private string VisitReturn(CReturn ret) {
        return $"Global.DEW_RESULT = {VisitExpr(ret.expr)};";
    }

    private string VisitFor(CFor For) {
        if (For.leaves().Count == 0)
            return "-all";
        
        foreach (Leaf line in For.expr1) 
            Write(VisitLine(line));
        Write($"While({(For.expr2 != null ? VisitExpr(For.expr2) : "True")});");
        ident++;
        foreach (Leaf line in For.leaves()) 
            Write(VisitLine(line));
        Write(For.expr3 != null ? VisitLine(For.expr3) : "-all");
        ident--;
        Write("End;");
        return "-all";
    }
    
    private string VisitWhile(CWhile While) {
        if (While.leaves().Count == 0)
            return "-all";
        
        Write($"While({VisitExpr(While.expr)});");
        ident++;
        foreach (Leaf line in While.leaves()) 
            Write(VisitLine(line));
        ident--;
        Write("End;");
        return "-all";
    }
    
    private string VisitIf(CIf If) {
        string ifexpr = VisitExpr(If.expr), elseexpr;
        if (ifexpr == "True") {
            foreach (Leaf line in If.leaves()) 
                Write(VisitLine(line));
            return "-all";
        }
        
        if (ifexpr == "False" || If.leaves().Count == 0)
            goto elses;

        Write($"If({ifexpr});");
        ident++;
        
        foreach (Leaf line in If.leaves()) 
            Write(VisitLine(line));
        
        elses:
        bool usedElseIfExpr = false;
        foreach (Leaf elifLeaf in If.Elifs) {
            CElif elif = (CElif)elifLeaf;
            elseexpr = VisitExpr(elif.expr);
            if (elseexpr == "True") {
                if (usedElseIfExpr || ifexpr != "False")
                    ident--;
                Write($"Else;");
                ident++;
                foreach (Leaf line in elif.leaves()) 
                    Write(VisitLine(line));
                ident--;
                
                Write("End;");
                return "-all";
            }
            
            if(elseexpr == "False" || elseexpr.Length == 0)
                continue;

            if (usedElseIfExpr || ifexpr != "False") {
                ident--;
                Write($"Else If({elseexpr});");
            }
            else 
                Write($"If({elseexpr});");

            ident++;
            foreach (Leaf line in elif.leaves()) 
                Write(VisitLine(line));
            usedElseIfExpr = true;
        }

        if (If.Else != null) {
            CElse Else = (CElse)If.Else;
            if (Else.leaves().Count == 0) {
                if(ifexpr != "False" || usedElseIfExpr)
                    Write("End;");
                return "-all";
            }

            if (usedElseIfExpr == false && ifexpr == "False") {
                foreach (Leaf line in Else.leaves()) 
                    Write(VisitLine(line));
                return "-all";
            }
                
            ident--;
            Write($"Else;");
            ident++;
            foreach (Leaf line in Else.leaves()) 
                Write(VisitLine(line));
        }
        
        ident--;
        Write("End;");
        return "-all";
    }
}