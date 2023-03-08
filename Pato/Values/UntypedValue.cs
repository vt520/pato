using System.Text.RegularExpressions;
using Pato;
using Pato.Processors;

namespace Pato.Values {
    public class UntypedValue : PatternProcessor {
        public override float DefaultConfidence => Confidence.Minimal;
        protected UntypedValue() : base(new Pattern {
            Regex = new Regex(@"(?:\s*(?<value>\S.*)\s*)", RegexOptions.ExplicitCapture),
            Normal = "${value}"
        }) { }
    }
}
