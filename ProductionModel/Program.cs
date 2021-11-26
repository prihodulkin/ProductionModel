using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionModel
{
    class Program
    {
        static void PrintForwardSearchResult(IEnumerable<int> initialFactsID, IEnumerable<int> terminalsID)
        {
            Graph graph = new Graph("facts.txt", "rules.txt");
            var tSet =   new HashSet<int>(terminalsID);
            var res = graph.ForwardSearch(initialFactsID, tSet);
            for (int i = 0; i < res.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {res[i]} \n\n");
            }
            if (tSet.Contains(res.Last().FindedFacts.Last().FactID))
            {
                Console.WriteLine($@"Forward search for fact(s) {String.Join(", ", terminalsID.ToArray())} 
                                    finish with success!!!\n");
            }
            else
            {
                Console.WriteLine($@"Forward search for fact(s) {String.Join(", ", terminalsID.ToArray())} 
                                    finish without success (((\n");
            }
        }

        static void PrintReverseSearchResult(IEnumerable<int> initialFactsID, int terminalID)
        {
            Graph graph = new Graph("facts.txt", "rules.txt");
            var res = graph.ReverseSearch(initialFactsID, terminalID);
            for (int i = 0; i < res.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {res[i]} \n\n");
            }
            if (res.Count>0)
            {
                Console.WriteLine($@"Reverse search for fact {terminalID} finish with success!!!\n");
            }
            else
            {
                Console.WriteLine($@"Reverse search for fact {terminalID} finish without success!!!\n");
            }
        }

        static void Run(string[] args)
        {
            if (args[0] == "f")
            {
                PrintForwardSearchResult(new int[] { 1, 2, 3, 4 }, args.Skip(1).Select(x => int.Parse(x)));
            } else if (args[0] == "r")
            {
                PrintReverseSearchResult(new int[] { 1, 2, 3, 4 }, int.Parse(args[1]));
            }
            else
            {
                PrintReverseSearchResult(new int[] { 1, 2, 3, 4 }, int.Parse(args[0]));
            }
            Console.ReadKey();
        }

        static void Main(string[] args)
        {
           // PrintForwardSearchResult(new int[] { 1, 2, 4 }, new int[] { 100});
            PrintReverseSearchResult(new int[] { 1, 2, 3,  4 }, 100);
            Console.ReadKey();
            //Run(args);
        }
    }
}
