using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionModel
{
    public class Fact
    {
        public int FactID { get; }
        public string Description { get; }
        
        public Fact(int factID, string description)
        {
            FactID = factID;
            Description = description;
        }

        public override int GetHashCode()
        {
            return FactID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if(obj is Fact)
            {
                return FactID == (obj as Fact).FactID;
            }
            return false;
        }

        public override string ToString()
        {
            return $"Fact id: {FactID}, decsription: {Description}";
        }

    }

    public class Rule
    {
        public int RuleID { get; }
        public string Description { get; }
        public HashSet<Fact> Causes { get; }
        public Fact Consequence { get; }



        public Rule(int ruleID, IEnumerable<Fact> causes, Fact consequence, string description)
        {
            RuleID = ruleID;
            Description = description;
            Causes = new HashSet<Fact>();
            foreach(var f in causes)
            {
                Causes.Add(f);
            }
            Consequence = consequence;
        }

        public override int GetHashCode()
        {
            return RuleID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Rule)
            {
                return RuleID == (obj as Rule).RuleID;
            }
            return false;
        }

        public bool MightBeApplied(HashSet<int> findedFactsID)
        {
            foreach (var fact in Causes)
            {
                if (!findedFactsID.Contains(fact.FactID))
                {
                    return false;
                }
            }
            return true;
        }


        public override string ToString()
        {
            return $@"Rule id: {RuleID}, 
                      description: {Description},
                      {String.Join("; ", Causes.Select( f=>f.ToString()).ToArray())} --> {Consequence}";
        }

    }

    public class SearchSnapshot
    {           
        public HashSet<Fact> FindedFacts { get; }
        public Rule Rule { get; }

        public SearchSnapshot(HashSet<Fact> findedFacts, Rule rule)
        {
            FindedFacts = findedFacts;
            Rule = rule;
        }

        public override string ToString()
        {
            return $@"Apllying of {Rule}. 
                    Facts: {String.Join(", ", FindedFacts.Select(f => f.FactID.ToString()).ToArray())}";
        }

    }


    public class Graph
    {
        Dictionary<int, Fact> facts;
        Dictionary<int, HashSet<Rule>> rules;

        public Graph()
        {
            facts = new Dictionary<int, Fact>();
            rules = new Dictionary<int, HashSet<Rule>>();
        }

        public Graph(string factsPath, string rulesPath)
        {
            facts = new Dictionary<int, Fact>();
            rules = new Dictionary<int, HashSet<Rule>>();
            foreach(var f in Parser.ParseFacts(factsPath))
            {
                AddFact(f);
                rules[f.FactID] = new HashSet<Rule>();
            }
            foreach(var r in Parser.ParseRules(rulesPath, facts))
            {
                AddRule(r);
            }
        }

        public void AddFact(Fact fact)
        {
            Fact existingFact;
            bool contains = facts.TryGetValue(fact.FactID, out existingFact);
            if (contains)
            {
                throw new ArgumentException($"Ошибка при добавлении {fact}. Уже присутствует { existingFact} ");          
            }
            facts[fact.FactID] = fact;
        }

        void CheckFactContaining(int id)
        {
            if (!facts.ContainsKey(id))
            {
                throw new ArgumentException($"Нет факта c ID {id}");
            }
        }

        void CheckFactContaining(Fact fact)
        {
            if (!facts.ContainsKey(fact.FactID))
            {
                throw new ArgumentException($"Нет факта {fact}");
            }
        }


        public void AddRule(Rule rule)
        {
            CheckFactContaining(rule.Consequence);
            foreach(var fact in rule.Causes)
            {
                CheckFactContaining(fact);
            }
            rules[rule.Consequence.FactID].Add(rule);       
        }

        public List<SearchSnapshot> ForwardSearch(IEnumerable<int> initialFactsID, HashSet<int> terminalsID)
        {
            List<SearchSnapshot> result = new List<SearchSnapshot>();
            HashSet<int> finded = new HashSet<int>();
            foreach(var id in initialFactsID)
            {
                CheckFactContaining(id);
                finded.Add(id);
            }
            foreach(var id in terminalsID)
            {
                CheckFactContaining(id);
            }  
            SearchStart:
            foreach(var ruleSet in rules.Values)
            {
                foreach(var rule in ruleSet)
                {
                    if (rule.MightBeApplied(finded) && !finded.Contains(rule.Consequence.FactID))
                    {
                        finded.Add(rule.Consequence.FactID);
                        result.Add(new SearchSnapshot(new HashSet<Fact>(finded.Select(f => facts[f])), rule));
                        if (terminalsID.Contains(rule.Consequence.FactID)) goto Finish;
                        goto SearchStart;
                    }
                }
               
            }
            Finish:
            return result;
        }

        public List<SearchSnapshot> ReverseSearch(IEnumerable<int> initialFactsID, int terminalID)
        {
            List<SearchSnapshot> result = new List<SearchSnapshot>();
            HashSet<int> finded = new HashSet<int>();
            HashSet<int> initial = new HashSet<int>(initialFactsID);
            foreach (var id in initialFactsID)
            {
                CheckFactContaining(id);
                finded.Add(id);
            }
            CheckFactContaining(terminalID);
            Stack<int> stack = new Stack<int>();
            Queue<int> queue = new Queue<int>();
            Dictionary<int, HashSet<Rule>> alternativeRules = new Dictionary<int, HashSet<Rule>>(
               rules);
            Dictionary<int, Rule> currentRule = new Dictionary<int, Rule>();
            var branch = new HashSet<int>();
            var noSolution = false;
            if (rules.ContainsKey(terminalID))
            {
                stack.Push(rules[terminalID].Last().Consequence.FactID);
            }
            else
            {
                return new List<SearchSnapshot>();
            } 
            while (stack.Count > 0)
            {
                var id = stack.Pop();
                if (!alternativeRules.ContainsKey(id)&&!initial.Contains(id))
                {
                    while (stack.Count > 0 &&  alternativeRules[stack.Peek()].Count == 0)
                    {
                        branch.Remove(stack.Pop());
                    }
                    if (stack.Count > 0)
                    {
                        id = stack.Pop();
                        var rulesS = alternativeRules[id];
                        currentRule[id] = rulesS.Last();
                        rulesS.Remove(rulesS.Last());
                    }
                    else
                    {
                        noSolution = true;
                        break;
                    }
                }
                if (!currentRule.ContainsKey(id))
                {
                    var rulesS = alternativeRules[id];
                    if (rulesS.Count > 0)
                    {
                        currentRule[id] = rulesS.Last();
                        rulesS.Remove(rulesS.Last());
                    }
                    else
                    {
                        noSolution = true;
                        break;
                    }
                   
                }
                var rule = currentRule[id];
                if (branch.Contains(rule.RuleID)&&rule.Causes.All(f=>!finded.Contains(f.FactID)))
                {
                    while(stack.Count>0&&alternativeRules[stack.Peek()].Count==0)
                    {
                        branch.Remove(stack.Pop());
                    }
                    if (stack.Count > 0)
                    {
                        id = stack.Pop();
                        var rulesS = alternativeRules[id];
                        currentRule[id] = rulesS.Last();
                        rulesS.Remove(rulesS.Last());
                        rule = currentRule[id];
                    }
                    else
                    {
                        noSolution = true;
                        break;
                    }
                }
                branch.Add(rule.RuleID);
                var notFinded= rule.Causes.Where(f => !finded.Contains(f.FactID));
                if (notFinded.Count() == 0)
                {
                    branch.Remove(rule.RuleID);
                    finded.Add(rule.Consequence.FactID);
                    result.Add(new SearchSnapshot(new HashSet<Fact>(finded.Select(f => facts[f])), rule));
                }
                else
                {
                    stack.Push(id);
                    foreach(var f in notFinded)
                    {
                        stack.Push(f.FactID);
                    }
                }
            }
            return noSolution?new List<SearchSnapshot>(): result;
        }
    }
}
