using System.Text.RegularExpressions;
using System.Data;
using Pato.Processors;

namespace Pato.Values {
    public class FloatValue : NumberProcessor {
        public override ICollection<string> ValueAtoms => new List<string> { "number" };
        public override string PrepareValue(string value) {
            return Regex.Replace(base.PrepareValue(value), @"([+-])\s", "$1");
        }
        protected FloatValue() : base(new Pattern {
            Regex = new Regex(@"(?<number>[+-]?(?<integer>[0-9]+)(?:[.](?<decimal>[0-9]+))?)", RegexOptions.ExplicitCapture),
            Normal = "${number}"
        }) { }
        public override IEnumerable<Processor> CreatableFrom =>
            Instances.Values.Where(item => item != this && item.Atoms.Contains("number")).ToList();
        public override Atoms? CreateFrom(Atoms? atoms, bool using_source = false) {
            if (using_source && atoms is not null) atoms = CreateFromDictionary(Atomize(atoms.SourceValue));
            if (atoms is null) return null;

            if (atoms.Data.TryGetValue("number", out string? number_string)) {
                if (float.TryParse(number_string, out float number_float)) {
                    return new Atoms { Processor = this, Value = number_float.ToString() };
                }
            }
            return base.CreateFrom(atoms);
        }
    }
}
