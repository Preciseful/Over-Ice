namespace lang; 

public interface Tree {
    public List<Leaf> leaves();
    public string name();

    public Leaf? Find(Predicate<Leaf> predicate) {
        Tree? tree = this;
        while (tree != null) {
            Leaf? leaf = tree.leaves().Find(l => predicate.Invoke(l));
            if (leaf == null)
                leaf = (tree as CFunction)?.parameters.Find(l => predicate.Invoke(l));
            if (leaf == null && tree as CFor != null) 
                // only expr1 is a line, and we're probably searching for a var 99% of the time
                leaf = (tree as CFor)!.expr1.Find(e => predicate.Invoke(e));

            if (leaf != null) return leaf;
            if ((tree as Leaf)?.tree as CProgram != null)
                break;
            tree = (tree as Leaf)?.tree;
        }
        
        if(tree == null)
            return null;
        
        CBody mainbody = (CBody)tree;
        Tree mainProgram = (tree as Leaf)!.tree!;

        if (mainbody.ImportPaths.Count > 0) {
            foreach (Leaf body in mainProgram.leaves()) {
                if(!mainbody.ImportPaths.Contains(body.name()))
                    continue;

                Leaf? leaf = (body as Tree)!.Find(predicate, new List<string>() { mainbody.name() });
                if (leaf != null) return leaf;
            }
        }

        return null;
    }
    
    public Leaf? Find(Predicate<Leaf> predicate, List<string> paths) {
        Tree tree = this;
        Leaf? leaf;
        foreach (Leaf treeleaf in tree.leaves()) {
            if (!predicate.Invoke(treeleaf))
                continue;
            if (treeleaf as CVariable != null && (treeleaf as CVariable)!.attributes.Exists(a => a.name == "public"))
                return treeleaf;
            if (treeleaf as CFunction != null && (treeleaf as CFunction)!.attributes.Exists(a => a.name == "public"))
                return treeleaf;
            if (treeleaf as CEnum != null && (treeleaf as CEnum)!.attributes.Exists(a => a.name == "public"))
                return treeleaf;
        }
        
        if((tree as Leaf)?.tree == null)
            return null;
        
        CBody mainbody = (CBody)tree;
        tree = (tree as Leaf)!.tree!;
        
        paths.Add(mainbody.name());

        if (mainbody.ImportPaths.Count > 0) {
            foreach (Leaf body in tree.leaves()) {
                if (!mainbody.ImportPaths.Contains(body.name()))
                    continue;
                if(paths.Contains(body.name()))
                    continue;
                leaf = (body as Tree)!.Find(predicate, paths);
                if (leaf != null) return leaf;
            }
        }

        return null;
    }
    
    public List<Leaf> FindAll(Predicate<Leaf> predicate) {
        List<Leaf> leaves = new List<Leaf>();
        Tree? tree = this;
        while (tree != null) {
            List<Leaf> leaf = tree.leaves().FindAll(l => predicate.Invoke(l));
            if (leaf.Count == 0) {
                Leaf? cv = (tree as CFunction)?.parameters.Find(l => predicate.Invoke(l));
                if(cv != null)
                    leaves.Add(cv);
            }

            leaves.AddRange(leaf);
            if ((tree as Leaf)?.tree as CProgram != null)
                break;
            tree = (tree as Leaf)?.tree;
        }
        
        if(tree == null)
            return leaves;
        
        CBody mainbody = (CBody)tree;
        Tree mainProgram = (tree as Leaf)!.tree!;

        if (mainbody.ImportPaths.Count > 0) {
            foreach (Leaf body in mainProgram.leaves()) {
                if (!mainbody.ImportPaths.Contains(body.name()))
                    continue;

                leaves.AddRange((body as Tree)!.FindAll(predicate, new List<string>() { mainbody.name() }));
            }
        }

        return leaves;
    }
    
    public List<Leaf> FindAll(Predicate<Leaf> predicate, List<string> paths) {
        Tree tree = this;
        List<Leaf> leaves = tree.leaves().FindAll(l => predicate.Invoke(l) && (
                                          (l as CFunction != null && (l as CFunction)!.attributes.Exists(a => a.name == "public")) ||
                                          (l as CEnum != null && (l as CEnum)!.attributes.Exists(a => a.name == "public")) ||
                                          (l as CVariable != null && (l as CVariable)!.attributes.Exists(a => a.name == "public"))));
        if ((tree as Leaf)?.tree == null)
            return leaves;
        
        CBody mainbody = (CBody)tree;
        tree = (tree as Leaf)!.tree!;
        
        paths.Add(mainbody.name());

        if (mainbody.ImportPaths.Count > 0) {
            foreach (Leaf body in tree.leaves()) {
                if (!mainbody.ImportPaths.Contains(body.name()))
                    continue;
                if(paths.Contains(body.name()))
                    continue;
                leaves.AddRange((body as Tree)!.FindAll(predicate, paths));
            }
        }

        return leaves;
    }
}