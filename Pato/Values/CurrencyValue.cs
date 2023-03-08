using System.Text.RegularExpressions;
using Pato;
using Pato.Processors;

namespace Pato.Values {
    public class CurrencyValue : NumberProcessor {
        protected CurrencyValue() : base(new Pattern {
            Regex = new Regex(@"(?:(?<currency_sign>[$])(?<number>(?<integer>[+-]?[0-9]+)(?:[.](?<decimal>[0-9]+))?))", RegexOptions.ExplicitCapture),
            Normal = "${currency_sign}${number}"
        }) { }
    }
}
