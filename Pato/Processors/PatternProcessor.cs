using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Pato.Processors {
    public class PatternProcessor : Processor {
        private IEnumerable<Pattern> Patterns;
        protected PatternProcessor(Pattern pattern) : this(new Pattern[] { pattern }) { }
        protected PatternProcessor(IEnumerable<Pattern> patterns) {
            Patterns = patterns;
        }
        public override string? GetAtom(string source_value, string? key = null) {
            key ??= NormalValue;
            return Atomize(source_value)[key];
        }
        public override ICollection<string> Atoms => GetPatternGroups();
        public override ICollection<string> ValueAtoms => GetCommonPatternGroups();
        public override string DefaultNormal => Patterns.FirstOrDefault()?.Normal ?? base.DefaultNormal;
        public override Score? Score(string? source_value) {
            if (source_value is not null) {
                string working_value = PrepareValue(source_value);
                int source_length = source_value.Length;
                foreach (Pattern pattern in Patterns) {
                    if (pattern.Regex.Match(working_value) is Match match && match.Success) {
                        int primary_match_length = match.Groups[0].Length;
                        int captures_count = 0;
                        int captures_match = 0;
                        foreach (Group group in match.Groups) {
                            if (group.Success) {
                                foreach (Capture capture in group.Captures) {
                                    captures_count++;
                                    if (capture.Length > 0) captures_match++;
                                }
                            } else {
                                captures_count++;
                            }
                        }
                        float primary_match = primary_match_length / (float)source_length;
                        float confidence = captures_match / (float)captures_count;
                        float score = primary_match * confidence;
                        return new Score(this) {
                            Confidence = DefaultConfidence,
                            Value = score
                        };
                    }
                }
            }
            return null;
        }
        private ICollection<string> GetCommonPatternGroups() {
            ICollection<string> groups = GetPatternGroups();
            foreach (Pattern pattern in Patterns) {
                groups = groups.Intersect(pattern.Regex.GetGroupNames()).ToList();
            }
            return groups;
        }
        private ICollection<string> GetPatternGroups() {
            Collection<string> groups = new();
            foreach (Pattern pattern in Patterns) {
                foreach (string group in pattern.Regex.GetGroupNames()) {
                    if (group == "0") continue;
                    if (!groups.Contains(group)) groups.Add(group);
                }
            }
            return groups;
        }

        public virtual IDictionary<string, string?> NormalizeValues(IDictionary<string, string?> values)
            => values;
        public override string? FormatValue(string source_value) {
            string working_value = PrepareValue(source_value);
            foreach (Pattern pattern in Patterns) {
                if (pattern.Regex.Match(working_value) is Match match && match.Success) {
                    IDictionary<string, string?> values = NormalizeValues(match.ToDictionary());
                    return FormatValue(values, pattern.Normal);
                }
            }
            return null;
        }
        public override string? FormatValue(IDictionary<string, string?>? values, string? normal = null) {
            normal ??= Patterns.First().Normal;
            return normal.Format(values);
        }
        public override IDictionary<string, string?> Atomize(string source_value) {
            IDictionary<string, string?> results = base.Atomize(source_value);
            string working_value = results[NormalValue]!;
            if (results[NormalValue] is not null) {
                foreach (Pattern pattern in Patterns) {
                    if (pattern.Regex.Match(working_value) is Match match && match.Success) {
                        foreach ((string name, string? value) in match.ToDictionary()) {
                            if (!results.TryAdd(name, value)) results[name] = value;
                        }
                        break;
                    }
                }
            }
            return NormalizeValues(results);
        }
    }
}
