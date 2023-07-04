namespace lang; 

public class CEvent {
    public string name;
    public string? hero, slot, team;

    public CEvent(Node node) {
        name = node.source.name();
        if(name == "globalized")
            return;
        
        hero = node.Get<Rule.Event.EventArg>(c => c.source.name() == "hero")?.Get(0)?.source.name() ?? "";
        slot = node.Get<Rule.Event.EventArg>(c => c.source.name() == "slot")?.Get(0)?.source.name() ?? "";
        if(hero != "" && slot != "")
            Compiler.Error("Cannot declare both hero and slot.", node);
        else if (hero == "" && slot == "")
            hero = "All";

        team = node.Get<Rule.Event.EventArg>(c => c.source.name() == "team")?.Get(0)?.source.name() ?? "All";
    }
}