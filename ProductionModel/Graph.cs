﻿using System;
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
            if (rules.ContainsKey(rule.Consequence.FactID))
            {
                rules[rule.Consequence.FactID].Add(rule);
            }
            else
            {
                rules.Add(rule.Consequence.FactID, new HashSet<Rule>() { rule});
            }
            
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
            Stack<int> alternativeWaysStack = new Stack<int>();
            HashSet<int> visited = new HashSet<int>(finded);
            var noSolution = false;
            stack.Push(terminalID);
            while (stack.Count > 0)
            {
                var id = stack.Pop();
                if (visited.Contains(id)||finded.Contains(id)) continue;
                if (!rules.ContainsKey(id)&&!initial.Contains(id))
                {
                    if (alternativeWaysStack.Count > 0)
                    {
                        var way = alternativeWaysStack.Pop();
                        if (alternativeWaysStack.Count == 0)
                        {
                            noSolution = true;
                            break;
                        }
                        while (stack.Pop() != way) ;
                        stack.Push(alternativeWaysStack.Peek());
                        continue;
                    }
                    noSolution = true;
                    break;
                }
                var rulesSet = rules[id];
                var rule = rulesSet.Last(); //!!!!!!!!!!
                if (rulesSet.Count > 1)
                {
                    foreach(var r in rulesSet)
                    {
                        alternativeWaysStack.Push(r.Consequence.FactID);
                    }
                }
                var notFinded= rule.Causes.Where(f => !finded.Contains(f.FactID));
                if (notFinded.Count() == 0)
                {
                    visited.Remove(id);
                    finded.Add(rule.Consequence.FactID);
                    result.Add(new SearchSnapshot(new HashSet<Fact>(finded.Select(f => facts[f])), rule));
                }
                else
                {
                    visited.Add(id);
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