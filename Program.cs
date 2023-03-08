using System.Linq.Expressions;
using System.Collections.Generic;
using Testing.Pato;
using Testing.Pato.Values;

namespace Pato
{
    internal class Program {
        static async Task Main(string[] args) {
            
            List<string> list = new() { "raisins", "suck", "hard" };
            Dictionary<string, string?> dict = list.ToDictionary<string, string, string?>(key => key, value => null);
            Processor p = Processor.ProcessorFor("Hello there");
            Atoms q = new Atoms { Value = "A sentance with !100 numbers and punctuation", Processor = typeof(UntypedValue)};
            Atoms r = new Atoms { Value = "A sentance with ~100!! nUmbers and Punctuation" };
            Atoms s = new Atoms { Value = "+100" };
            Atoms t = new Atoms { Value = "1.100" };
        }
    }
}