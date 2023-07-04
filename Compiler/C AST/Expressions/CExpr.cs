using System.Text.RegularExpressions;

namespace lang; 
using Newtonsoft.Json.Linq;

public static class CExpr {
    public static Leaf Convert(Node node, Tree tree, bool compileConstant = false) {
        if (!compileConstant) {
            if (node.Hold as Identifier != null) {
                Leaf? ident = tree.Find(l => l.name() == node.source.name() && 
                                             (l as CVariable != null || 
                                              l as CEnum != null ||
                                              l as LambdaVar != null));
                if (ident == null) {
                    if (Compiler.Enums.TryGetValue(node.source.name(), out var en))
                        return CEnum.UnpackJSON(node.source.name(), en.ToObject<JObject>()!);
                    Compiler.Error("Unknown identifier.", node);
                }
                
                tree.leaves().Add(ident);
                return ident;
            }
            if (node.Hold as Expr.InvokeExpr != null)
                return CInvokeExpr.Unpack(node, tree);
            if (node.Hold as Expr.StaticInvokeExpr != null)
                return CStaticInvokeExpr.Unpack(node, tree);
            if (node.Hold as Call != null)
                return CCall.Unpack(node, tree);
            if (node.Hold as Array.IndexAccess != null)
                return CArray.UnpackIndexAccess(node, tree);
            if (node.Hold as Expr.LambdaExpr != null)
                return CLambda.Unpack(node, tree);
        }
        if (node.Hold as Expr.BinaryExpr != null)
            return new CBinaryExpr(node, tree, compileConstant);
        if (node.Hold as Expr.UnaryExpr != null)
            return new CUnaryExpr(node, tree, compileConstant);
        if (node.Hold as int? != null)
            return new CInt(node.source.name(), (int)node.Hold, tree);
        if (node.Hold as float? != null)
            return new CFloat(node.source.name(), (float)node.Hold, tree);
        if (node.Hold as Expr.Null != null)
            return new CNull(tree);
        if (node.Hold as Expr.Bool != null)
            return new CBool(node.source.name(), node.source.name() == "true", tree);
        if (node.Hold as Array != null)
            return CArray.Unpack(node, tree, compileConstant);
        if (node.Hold as Expr.Literal != null)
            return new CString(node.source.name(), tree);

        if(compileConstant)
            Compiler.Error("Value must be a constant during compile-time.", node);
        Compiler.Error("Invalid expression value.", node);
        return null;
    }
    
    public static bool IsAtom(Node node) => 
        node.Hold as Expr.Literal != null || node.Hold as Identifier != null || node.Hold as Call != null;
    
    public static bool IsAtom(Leaf leaf) => 
        leaf as CVariable != null || leaf as CCall != null || leaf as CString != null;
    
    
    public static void VerifyTypes(Node node, Leaf lhv, Leaf rhv, bool assign) {
        if (lhv.type() == "obj" || rhv.type() == "obj")
            return;

        if (!VerifyTypes(lhv.type(), rhv.type())) {
            if (lhv.type().Contains("array") && assign) {
                string[] rhvarrays = rhv.type().Split(' ');
                
                // is an empty array
                if (rhvarrays.Last() != "array" && rhvarrays.Last() != "%")
                    Compiler.Error("Unmatching types.", node);
            }
            else 
                Compiler.Error("Unmatching types.", node);
        }
    }


    private static Dictionary<string, string[]> ConversableTypes = new Dictionary<string, string[]>() {
        { "int", new[] { "float", "long" } }
    };
    
        
    public static bool VerifyTypes(Leaf lhv, Leaf rhv) {
        // yes all im doing is just changing the order of the arguments
        if (GetParentType(lhv.type()) == "Act" && rhv as CLambda != null) 
            return CLambda.VerifyTypes((CVariable)lhv, (CLambda)rhv);
        if (GetParentType(rhv.type()) == "Act" && lhv as CLambda != null)
            return CLambda.VerifyTypes((CVariable)rhv, (CLambda)lhv);

        return VerifyTypes(lhv.type(), rhv.type());
    }
    
    public static bool VerifyTypes(string x, string y) {
        if (x == y)
            return true;

        if (ConversableTypes.ContainsKey(x) && ConversableTypes[x].Contains(y))
            return true;
        
        if (x.Contains("array") || y.Contains("array")) {
            string[] rhvarrays = y.Split(' '), lhvarrays = x.Split(' ');
                
            // is an empty array
            if (rhvarrays.Last() != "array" && lhvarrays.Last() != "array")
                return false;
            return true;
        }

        return false;
    }
    
    public static bool VerifyTypes(string x, JArray y) {
        foreach (JToken token in y) {
            if (VerifyTypes(x, token.ToString()))
                return true;
        }

        return false;
    }

    public static string GetParentType(string x) {
        if (!x.Contains('<'))
            return x;

        return x.Split('<')[0];
    }

    public static string[] GetChildTypes(string x) {
        if (!x.Contains('<'))
            return new string[] {};

        List<string> result = new List<string>();
        string child = x.Split('<', 1)[1], temp = "";
        int count = 0;
        
        foreach (char c in child) {
            if (c == child.Last()) {
                result.Add(temp);
                break;
            }

            if (c == '<')
                count++;
            else if (c == '>')
                count--;
            temp += c;

            if (count == 0 && c == ',') {
                result.Add(temp);
                temp = "";
            }
        }

        return result.ToArray();
    }
}