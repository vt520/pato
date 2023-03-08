using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Immutable;
using System.Data;
using Pato;

namespace Pato {
    public static class Extensions {
        public static int GetValueHashCode(this IDictionary dictionary, IEnumerable keys) {
            if (dictionary is null) return 0;
            int accumulator = 0;
            foreach (object key in keys) {
                if (dictionary.Contains(key) && dictionary[key] is object value) {
                    accumulator ^= value.GetHashCode();
                }
            }
            return accumulator;
        }
        public static int GetValueHashCode<K, V>(this IDictionary<K, V> dictionary, IEnumerable<K> keys) where K : notnull {
            if (dictionary is not IDictionary value) {
                value = ImmutableDictionary.CreateRange(dictionary);
            }
            return value.GetValueHashCode(keys);
        }
        public static T? ValueAs<T>(this IDictionary<string, string?>? source, string key) {
            if (source is null) return default;
            try {
                if (source.TryGetValue(key, out string? value)) {
                    if (typeof(T) == typeof(string)) return (T)(object)value!;
                    if (typeof(T).GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, new Type[] { typeof(string) }) is MethodInfo method) {
                        if (method.Invoke(null, new object?[] { value }) is T result) {
                            return result;
                        }
                    }
                    return default;
                }
                return default;
            } catch (TargetInvocationException ex) {
                if (ex.InnerException is not FormatException) throw ex.InnerException;
                return default;
            } catch {
                throw;
            }
            throw new EntryPointNotFoundException();
        }

        /// <summary>
        /// Returns true if all elements of objects are found in source
        /// </summary>
        /// <typeparam name="T">The type of object you are searching for</typeparam>
        /// <param name="source">The IEnumerable you're searching</param>
        /// <param name="objects">An IEnumerable containing the objects you're looking for</param>
        /// <returns>true if source contains all objects as value</returns>
        public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> objects) {
            foreach (T item in objects) {
                if (!source.Contains(item)) return false;
            }
            return true;
        }

        /// <summary>
        /// Extension method for selecting a processor for a given string value
        /// </summary>
        /// <param name="processors">An enumeration of currently instatiated processors</param>
        /// <param name="value">The string value you want to find a processor for</param>
        /// <returns>The processor with the highest Value adjusted by Confidence, 
        /// then by Confidence alone, then by Default Confidence, 
        /// and lastly by enumeration order</returns>
        public static Processor? SelectBest(this IEnumerable<Processor> processors, string value) {
            if (processors.Score(value) is IEnumerable<Score> scores) {
                if (scores.OrderByDescending(item => (float)item)
                .ThenByDescending(item => item.Confidence)
                .ThenByDescending(item => item.Processor.DefaultConfidence).FirstOrDefault() is Score score) {
                    return score.Processor;
                }
            }
            return null;
        }
        /// <summary>
        /// Returns a list of score objects describing the likelihood of a type match
        /// </summary>
        /// <param name="processors">An Enumeration of Instantiated Processors</param>
        /// <param name="value">The value you wish to inspect</param>
        /// <returns>An unordered list of Score objects describing the likelihood of a match for a given type</returns>
        public static IEnumerable<Score> Score(this IEnumerable<Processor> processors, string? value) {
            List<Score> scores = new();
            foreach (Processor processor in processors) {
                if (processor.Score(value) is Score score) scores.Add(score);
            }
            return scores;
        }
        /// <summary>
        /// Converts a Regex Match object into a dictionary with predictable characteristics
        /// </summary>
        /// <param name="match">A valid <see cref="Match"/> object</param>
        /// <param name="keys">An optional list of keys to supply to the initial dictionary</param>
        /// <param name="fixed_keys">If true; the resulting dictionary will be restricted to the keys present in <paramref name="keys"/></param>
        /// <returns>null if <paramref name="match"/> is null or unsuccessful; or a populated dictionary of value extracted from the 
        /// groups composing the first match
        /// </returns>
        public static IDictionary<string, string?> ToDictionary(this Match match, IEnumerable<string>? keys = null, bool fixed_keys = false) {
            Dictionary<string, string?> results = new();
            keys ??= new List<string>();
            foreach (string key in keys) results[key] = null;
            foreach (Group group in match.Groups) {
                if (group.Name == "0") continue;
                if (fixed_keys && !keys.Contains(group.Name)) continue;
                if (group.Captures.Count == 0) {
                    results[group.Name] = null;
                } else {
                    if (group.Captures.First() is Capture first_capture) {
                        if (!results.TryAdd(group.Name, first_capture.Value)) {
                            results[group.Name] ??= first_capture.Value;
                        }
                    }
                }
                if (group.Captures.Count > 1) {
                    for (int i = 0; i < group.Captures.Count; i++) {
                        string alt_name = group.Name + "." + i;
                        if (!results.TryAdd(alt_name, group.Captures[i].Value)) {
                            results[alt_name] ??= group.Captures[i].Value;
                        }
                    }
                    results.TryAdd(group.Name + ".count", group.Captures.Count.ToString());
                }
            }
            return results;
        }
        /// <summary>
        /// This is just for my happiness.
        /// </summary>
        /// <param name="template">A string optionally containing ${} tags</param>
        /// <param name="values">A Dictionary that contains the value for be formatted</param>
        /// <returns>A string with the ${} tags replaced for each key value in the dictionary</returns>
        public static string? Format(this string? template, IDictionary<string, string?>? values) {
            if (values is null) return null;
            template ??= string.Empty;
            foreach ((string key, string? value) in values) {
                string replacement = $"${{{key}}}";
                template = template.Replace(replacement, value);
            }
            return template;
        }
    }
}
