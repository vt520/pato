using System.Text.RegularExpressions;
using Pato;
using Pato.Processors;

namespace Pato.Values {
    public class WordsValue : PatternProcessor {
        public override float DefaultConfidence => Confidence.Normal;
        public override ICollection<string> ValueAtoms => new List<string> { "words" };
        public override string PrepareValue(string value) {
            value = Regex.Replace(value, @"[^\w\s]|[0-9]", "");
            value = Regex.Replace(value, @"\s\s+", " ");
            return value;
        }
        public override IDictionary<string, string?> NormalizeValues(IDictionary<string, string?> values) {
            foreach (string name in values.Keys) {
                values[name] = values[name]?.Trim().ToUpper();
            }
            return base.NormalizeValues(values);
        }
        protected WordsValue() : base(new Pattern {
            Regex = new Regex(@"\W?(?<words>(?:(?<word>\w+)\s*)+)\W*", RegexOptions.ExplicitCapture),
            Normal = "${words}"
        }) { }
    }
}
