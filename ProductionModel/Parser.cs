using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionModel
{
    class Parser
    {
        public static int ParseID(string idStr)
        {
            var s = idStr.Substring(2);
            return int.Parse(s);
        }

        public static Fact ParseFact(string s)
        {
            var arr = s.Split(';');
            if (arr.Length != 2)
            {
                throw new ArgumentException($"Некорректная сторка для факта: {s}");
            }
            return new Fact(ParseID(arr[0]), arr[1]);
        }

        public static IEnumerable<Fact> ParseFacts(IEnumerable<string> facts)
        {
            foreach(var fact in facts)
            {
                yield return ParseFact(fact);
            }
        }

        public static IEnumerable<Fact> ParseFacts(string filePath)
        {
            return ParseFacts(File.ReadLines(filePath));
        }

        public static Rule ParseRule(string s, Dictionary<int, Fact> facts)
        {
            var arr = s.Split(';');
            if (arr.Length != 4)
            {
                throw new ArgumentException($"Некорректная сторка для факта: {s}");
            }
            var ruleID = ParseID(arr[0]);
            var causes = arr[1].Split(',').Select(f=>facts[ParseID(f)]);
            var consequence = facts[ParseID(arr[2])];
            return new Rule(ruleID, causes, consequence, arr[3]);
        }

        public static IEnumerable<Rule> ParseRules(IEnumerable<string> rules, Dictionary<int, Fact> facts)
        {
            foreach (var rule in rules)
            {
                yield return ParseRule(rule, facts);
            }
        }

        public static IEnumerable<Rule> ParseRules(string filePath, Dictionary<int, Fact> facts)
        {
            return ParseRules(File.ReadLines(filePath), facts);
        }
    }
}
