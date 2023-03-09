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
        
    }
}
