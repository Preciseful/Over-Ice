namespace lang; 
using Newtonsoft.Json.Linq;

public class CEnum : Leaf {
    public class Invoke : Leaf {
        private string _type;
        
        public Tree? tree { get; set; }
        public string name() => _type;
        public string type() => _type;
        public Value value;
        public CEnum Enum;

        public Invoke(string _type, string valuename, CEnum Enum, Tree tree) {
            this._type = _type;
            this.Enum = Enum;
            this.value = this.Enum.values.Find(c => c.name() == valuename)!;
            this.tree = tree;
        }
    }
    
    public class Value {
        private string _name, _type;
        
        public string name() => _name;
        public string type() => _type;
        public int index;
        public bool isWorkshop;

        public Value(string name, string _type, int? index = null) {
            this._name = name;
            this._type = _type;
            if (index == null)
                isWorkshop = true;
            else
                this.index = index.Value;
        }

        public static Value Unpack(Node node, string type) {
            return new Value(node.source.name(), type, (int)node.Get(0)!.Hold);
        }
    }
    
    private string _type;

    public List<Value> values;
    public Tree? tree { get; set; }
    public string name() => _type;
    public string type() => _type;
    public List<CAttribute> attributes;

    public CEnum(string _type, List<Value> values, List<CAttribute> attributes, Tree? tree = null) {
        this._type = _type;
        this.tree = tree;
        this.attributes = attributes;
        tree?.leaves().Add(this);
        this.values = values;
    }

    public static CEnum Unpack(Node node, Tree? tree = null) {
        Node ident = node.Get<Identifier>()!,
            block = node.Get<Block>()!;
        List<Value> values = new List<Value>();
        foreach (Node value in block.Children) 
            values.Add(Value.Unpack(value, ident.source.name()));

        return new CEnum(ident.source.name(), values, CAttribute.Unpack(node.Get<Attribute.Attributes>()), tree);
    }

    public static CEnum UnpackJSON(string _type, JObject j) {
        var jarray = j["values"]!.ToObject<JObject>()!;
        
        List<Value> values = new List<Value>();
        foreach (var value in jarray) 
            values.Add(new Value(value.Key, _type));

        return new CEnum(_type, values, new List<CAttribute>());
    }
}
