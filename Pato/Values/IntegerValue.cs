using System.Text.RegularExpressions;
using Pato.Processors;

namespace Pato.Values {
    public class IntegerValue : NumberProcessor {
        public override ICollection<string> ValueAtoms => new List<string> { "number" };
        public override int Compare(IDictionary<string, string?>? left, IDictionary<string, string?>? right) {
            int left_value = (int)left.ValueAs<float>("number");
            int right_value = (int)right.ValueAs<float>("number");
            if (left_value < right_value) return -1;
            if (left_value > right_value) return 1;
            return 0;
        }
        protected IntegerValue() : base(new Pattern {
            Regex = new(@"(?<number>[+-]?(?<integer>[0-9]+))", RegexOptions.ExplicitCapture),
            Normal = "${number}"
        }) { }
    }
}
