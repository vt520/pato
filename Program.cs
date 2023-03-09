using System.Linq.Expressions;
using System.Collections.Generic;
using Pato.Values;
using System.Reflection;
using System.Diagnostics;

namespace Pato
{
    internal class Program {
        /// <summary>
        /// Using ¡Pato!
        /// 
        /// Nomenclature:
        ///     Atoms: a group of "Atoms" or "Facts" that are known to compose a Value, or inferred from it
        ///     Inference: a value not directly contained by the source value
        ///     Singleton: a class that should have one, and only one extant instance
        ///     Normalization: a value that should be assumed to be altered for standardization
        ///     Value: either a literal string, or a set of Atoms generated from a string
        ///     
        /// Note:
        ///     Properties in Pato are lazy loaded, calculated, and cached as the rule, rather than the exception
        ///     ie: Never assume a computation has been done if you've not looked at the results
        ///     
        /// Basics:
        ///     The value object in Pato is the Atoms class which represents a single string
        ///     and the inferred meanings (Atoms) identified by one or more Processors that 
        ///     recognize that specific value, or inferred by the Atoms structure.
        /// 
        ///     The value Atoms are created and managed by the singleton Processor derived classes; while the 
        ///     instantiation of the classes themselves is handled using the Processor class directly.
        /// 
        ///     When creating a value of a known type, you can either use the derived Processor's Atomize method 
        ///     or directly create an Atoms object with the Processor type or Instance
        /// 
        ///     To determine the type of an unknown value, you can use the ProcessorFor static method to obtain the 
        ///     most likely processor for a given string
        /// 
        ///     To allow Pato to automatically determine the Processor for a given Atoms object, leave the 
        ///     Processor value unset or set to null; or create the Atoms using the static Parse method
        /// 
        ///     Every Atoms object contains a read-write Value property, that when is processed by the Processor 
        ///     when set and returns a Normalized value when read; along with a read-only SourceValue property 
        ///     that contains the value given to the Value property without change.
        /// 
        /// Atoms:
        ///     Atoms present the available facts through several properties:
        ///       * the ValueAtoms property contains the facts what when set alter the Value property
        ///       * the CurrentAtoms property contains the list of facts that are currently loaded into this Atoms
        ///       * the PotentialAtoms property contains a list of all knowable facts about the current Atoms
        ///         
        ///     Atoms present their fully realized facts (inferred and defined) with a Dictionary provided by the 
        ///     Data property
        ///         
        ///     To access a given fact, use the ValueOf method of the Atoms object
        ///     (Note: Once a fact has been fully realized, it is present in the Data property, 
        ///     but new facts cannot be directly realized using this property)
        ///         
        ///     To set the value of a fact, write the desired value to the Data dictionary.  If the fact is one listed
        ///     in the ValueAtoms property, all existing facts will be forgotten replaced with ones found using
        ///     the Normalized value of the ValueAtoms
        ///         
        ///     Setting a fact for any name not present in the ValueAtoms does not effect local evaluation or the 
        ///     Normalized form of the Value
        ///         
        ///     Every Atoms object has an associated Processor; which if unstated or null will be inferred from 
        ///     the current value contained in SourceValue
        ///     
        /// Processors:
        ///     
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static Task Main(string[] args) {
            string hello = "Hello There!";
            Processor o = Processor.ProcessorFor(hello);
            Atoms p = new Atoms { Value = hello, Processor = o };
            Atoms q = Processor.Parse("A sentance with !100 numbers and punctuation")!;
            q.Processor = typeof(UntypedValue);
            Atoms r = new Atoms { Value = "A sentance with ~100!! nUmbers and Punctuation" };
            Atoms s = new Atoms { Value = "+100", Processor = default! };
            Atoms t = new Atoms { Value = "1.100" };
            Atoms u = Processor.InstanceOf<UntypedValue>().CreateFrom(s, true)!;
            Console.WriteLine((p.Value, q.Value, r.Value, s.Value, t.Value, u.Value));
            return Task.CompletedTask;
        }
    }

}