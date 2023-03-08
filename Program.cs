using System.Linq.Expressions;
using System.Collections.Generic;
using Pato.Values;

namespace Pato
{
    internal class Program {
        static async Task Main(string[] args) {
            
            Processor p = Processor.ProcessorFor("Hello there");
            Atoms q = new Atoms { Value = "A sentance with !100 numbers and punctuation", Processor = typeof(UntypedValue)};
            Atoms r = new Atoms { Value = "A sentance with ~100!! nUmbers and Punctuation" };
            Atoms s = new Atoms { Value = "+100" };
            Atoms t = new Atoms { Value = "1.100" };
            Atoms? u = Processor.InstanceOf<UntypedValue>().CreateFrom(t);
        }
    }
}