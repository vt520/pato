using System.Text.RegularExpressions;

namespace Pato {
    public class Pattern {
        public static implicit operator Regex(Pattern pattern) => pattern.Regex;
        public static implicit operator Pattern(Regex regex) => new() { Regex = regex, Normal = "$0" };
        public Regex Regex { get; set; } = null!;
        public string? Normal { get; set; }
    }
}
