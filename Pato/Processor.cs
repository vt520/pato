using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;
using System.Collections.Immutable;
using System.Data;
using Pato.Values;

namespace Pato {
    /// <summary>
    /// The Processor class contains the basic implementation for a Type that can by processed.
    /// By convention, the Processor class is a Singleton, and an initialized object can be obtained through 
    /// the static Instance methods
    /// 
    /// An implementation of my Enzyme pattern
    /// </summary>
    public abstract class Processor {
        /// <summary>
        /// QOL Operator that allows fetches an instance of a given type from the global pool
        /// </summary>
        /// <param name="type">The type of instance you want</param>
        public static implicit operator Processor(Type type) => InstanceOf(type);
        /// <summary>
        /// QOL Operator for accessing a Processor Instance from a string label.
        /// </summary>
        /// <param name="name">The name of the processor instance</param>
        public static explicit operator Processor(string name) {
            if (Instances.Keys.Where(item => item.FullName == name).FirstOrDefault() is Type qualified_type) {
                return InstanceOf(qualified_type);
            }
            if (Instances.Keys.Where(item => item.Name == name).FirstOrDefault() is Type unqualified_type) {
                return InstanceOf(unqualified_type);
            }
            if (Instances.Keys.Where(item => item.Name == $"{name}Value").FirstOrDefault() is Type short_type) {
                return InstanceOf(short_type);
            }
            throw new EntryPointNotFoundException();
        }
        /// <summary>
        /// The backing field for Instances
        /// </summary>
        private static ConcurrentDictionary<Type, Processor> _Instances = new();
        /// <summary>
        /// A bag containing a list of assemblies that we've already searched for objects
        /// </summary>
        private static ConcurrentBag<Assembly> SearchedAssemblies = new();
        /// <summary>
        /// Checks to see if there are any new assemblies present in the Current Application Domain 
        /// </summary>
        private static bool InstancesValid =>
            SearchedAssemblies.Count() == AppDomain.CurrentDomain.GetAssemblies().Count();
        /// <summary>
        /// Returns a list of assemblies that are present in the Current Domain, but are not in the Bag of Already checked assemblies
        /// </summary>
        public static IEnumerable<Assembly> UncheckedAssemblies => AppDomain.CurrentDomain.GetAssemblies().Except(SearchedAssemblies);
        /// <summary>
        /// Returns a read-only copy of the global instance cache
        /// </summary>
        protected static IDictionary<Type, Processor> Instances {
            get {
                if (!InstancesValid) {
                    foreach (Assembly assembly in UncheckedAssemblies) {
                        foreach (Type type in assembly.GetTypes()) {
                            if (!type.IsAbstract && type.IsAssignableTo(typeof(Processor))) {
                                if (type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, Type.EmptyTypes) is ConstructorInfo constructor) {
                                    if (constructor.Invoke(null) is Processor created) {
                                        _Instances.TryAdd(type, created);
                                        /// update conversion map here
                                    }
                                }
                            }
                        }
                    }
                }
                return _Instances.ToImmutableDictionary();
            }
        }
        /// <summary>
        /// Represents standard levels of confidence that can be used or adjusted
        /// </summary>
        public struct Confidence {
            public const float Normal = 0.9f;
            public const float Low = 0.8f;
            public const float Ignore = 0f;
            public const float High = 1f;
            public const float Lower = 0.5f;
            public const float Minimal = 0.01f;
        }
        /// <summary>
        /// The atom name that is used as a default normal value;
        /// </summary>
        public const string NormalValue = "value";
        public virtual string DefaultNormal => $"${{{" + NormalValue + "}}}";
        /// <summary>
        /// Returns a default Processor object, by convention this should be an object you _don't_ want to use in this case it's an EmptyValue
        /// </summary>
        public static Processor Default { get; internal set; } = InstanceOf<EmptyValue>();
        /// <summary>
        /// Gets a specific object from the global instance pool
        /// </summary>
        /// <typeparam name="T">The Processor Type to Fetch</typeparam>
        /// <returns>Returns a Processor object</returns>
        public static Processor InstanceOf<T>() where T : Processor => InstanceOf(typeof(T));
        /// <summary>
        /// Creates or Returns a Processor of a given type
        /// </summary>
        /// <param name="processor_type">The Type of Processor</param>
        /// <returns>A Processor object from the Global Pool</returns>
        /// <exception cref="NullReferenceException">If the processor could not be created or fetched</exception>
        public static Processor InstanceOf(Type processor_type) {
            if (!Instances.TryGetValue(processor_type, out Processor? instance)) {
                if (Activator.CreateInstance(processor_type, true) is Processor created) {
                    instance = created;
                    if (!Instances.TryAdd(processor_type, instance)) return InstanceOf(processor_type);
                }
            }
            if (instance is null) throw new NullReferenceException();
            return instance;
        }
        /// <summary>
        /// Returns the best scored processor for a given input text
        /// </summary>
        /// <param name="source_value">The value that you want to process</param>
        /// <returns>The best matched processor for the input given</returns>
        public static Processor ProcessorFor(string source_value) {
            return Instances.Values.SelectBest(source_value) ?? Default;
        }
        /// <summary>
        /// Create a new processor object
        /// </summary>
        protected Processor() { }
        /// <summary>
        /// An Empty Atoms object for this Processor
        /// </summary>
        public virtual IDictionary<string, string?> DefaultAtoms {
            get {
                Dictionary<string, string?> result = new();
                foreach (string atom in Atoms) result[atom] = null;
                return result;
            }
        }
        /// <summary>
        /// Tests if the given Atoms are compatible with this processor
        /// </summary>
        /// <param name="atoms">The Atoms to check for compatability</param>
        /// <returns>true if IsCompatible, false otherwise</returns>
        public bool IsCompatible(Atoms? atoms) {
            if (atoms is not null) {
                return IsCompatible(atoms.Processor);
            }
            return false;
        }
        /// <summary>
        /// Tests if a Processor is compatible with another Processor
        /// </summary>
        /// <param name="processor">The Processor object to check for compatability</param>
        /// <returns>true if IsCompatible, false otherwise</returns>
        public bool IsCompatible(Processor? processor) {
            if (processor is null) return false;
            if (processor == this) return true;
            if (processor.Atoms.ContainsAll(ValueAtoms)) return true;
            return false;
        }
        /// <summary>
        /// Returns a list of Processors that this Processor can natively create value for
        /// </summary>
        /// <returns>a list of Processors</returns>
        public virtual IEnumerable<Processor> ConvertsTo() => new List<Processor>();
        /// <summary>
        /// Tests if a Processor is in the ConvertsTo list
        /// </summary>
        /// <param name="processor"></param>
        /// <returns></returns>
        public bool ConvertsTo(Processor processor) => ConvertsTo().Contains(processor);
        /// <summary>
        /// A list of Processors that contain value that can be used to create new Atoms of this type
        /// </summary>
        /// <returns>A list of Processor objects</returns>
        public virtual IEnumerable<Processor> CreatableFrom => new List<Processor>();
        /// <summary>
        /// Tests if a new set of Atoms from this type can be created from the supplied Atoms
        /// </summary>
        /// <param name="atoms">The value to test</param>
        /// <returns>true if this Processor can use the supplied values</returns>
        public virtual bool IsCreatableFrom(Atoms atoms) => IsCreatableFrom(atoms.Processor);
        /// <summary>
        /// Tests if the given dictionary contains all of the keys necessary to create Atoms using this processor
        /// </summary>
        /// <param name="values">A Dictionary of proposed values</param>
        /// <returns>true if this dictionary is found to be acceptable</returns>
        public virtual bool CanCreateFromDictionary(IDictionary<string, string?>? values) {
            values ??= new Dictionary<string, string?>();
            return values.Keys.ContainsAll(ValueAtoms);
        }

        /// <summary>
        /// Tests if a given Processor produces Atoms which can be used to create Atoms of this Processor
        /// This method also tests if an intermediate object can create be used in the transformation
        /// </summary>
        /// <param name="processor">The Processor to test for compatability</param>
        /// <returns>true if the Processor can use the supplied Processors Atoms for the creation of new Atoms</returns>
        public virtual bool IsCreatableFrom(Processor processor) {
            if (CreatableFrom.Contains(processor)) return true;
            foreach (Processor converter in CreatableFrom) {
                if (converter.IsCreatableFrom(processor)) return true;
            }
            return false;
        }
        /// <summary>
        /// Attempt to create a new set of Atoms from the Atoms supplied
        /// 
        /// This should be overriden if you're directly creating Atoms
        /// </summary>
        /// <param name="atoms">The value you want to create from</param>
        /// <returns>a new Atoms value, if compatible, null otherwise</returns>
        public virtual Atoms? CreateFrom(Atoms? atoms) =>
            CreateFromDictionary(atoms?.Data) ?? new() { Processor = this, Value = atoms?.Value };
        /// <summary>
        /// Uses the default normal to create a value from values, 
        /// if it cannot create a value using the given dictionary, 
        /// it tests to see if a sub-type can handle the conversion
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public virtual Atoms? CreateFromDictionary(IDictionary<string, string?>? values) {
            values ??= new Dictionary<string, string?>();
            if (CanCreateFromDictionary(values)) {
                return new Atoms { Processor = this, Value = FormatValue(values, DefaultNormal) };
            }
            foreach (Processor converter in CreatableFrom.OrderBy(item => item.DefaultConfidence)) {
                if (converter.CanCreateFromDictionary(values)) {
                    return CreateFrom(
                        converter.CreateFromDictionary(values)
                    );
                }
            }
            return null;
        }
        /// <summary>
        /// Attempts to convert the supplied value to another type
        /// </summary>
        /// <typeparam name="T">The destination type</typeparam>
        /// <param name="atoms">The values to use to create the new Atoms</param>
        /// <returns>Null if no conversion can be found, Atoms otherwise</returns>
        public virtual Atoms? ConvertTo<T>(Atoms atoms) where T : Processor
            => ConvertTo(InstanceOf<T>(), atoms);
        /// <summary>
        /// Attempts to convert the supplied Atoms to the given Type
        /// </summary>
        /// <param name="processor">The destination type</param>
        /// <param name="atoms">The values to use</param>
        /// <returns>Atoms if the conversion succeeds, null otherwise</returns>
        public virtual Atoms? ConvertTo(Processor processor, Atoms? atoms)
            => ConvertTo(processor, atoms as IDictionary<string, string?>);

        /// <summary>
        /// Creates a new set of Atoms from the provided dictionary of the same type as produced by the named processor
        /// </summary>
        /// <param name="processor">The processor type for the Atoms</param>
        /// <param name="atoms">The values you wish to convert</param>
        /// <returns>Atoms if the conversion is sucessful, otherwise null</returns>
        public virtual Atoms? ConvertTo(Processor processor, IDictionary<string, string?>? atoms) => null;



        /*        public bool Converts(Processor processor) {
                    //wtf?
                    if (CanCreateFromDictionary(processor)) return true;
                    if (processor.ConvertsTo(this)) return true;
                    if (CanCreateFromDictionary(processor) || processor.ConvertsTo(this)) {
                        return true;
                    }
                    foreach (Processor subprocessor in CreatableFrom()) {
                        if (subprocessor.CanCreateFromDictionary(this)) { return true; }
                    }
                    return false;
                }
                public Atoms? Convert(Atoms? values) {
                    //wtf?
                    if (values is not null) {
                        if (CanCreateFromDictionary(values.Processor) && CreateFromDictionary(values) is Atoms converted_from) {
                            return converted_from;
                        }
                        if (values.Processor.ConvertsTo(this) && values.Processor.ConvertTo(this, values) is Atoms converted_to) {
                            return converted_to;
                        }
                        foreach (Processor intermediate in CreatableFrom()) {
                            if (intermediate.CanCreateFromDictionary(values)) return Convert(intermediate.CreateFromDictionary(values));
                        }
                    }
                    return null;
                }*/
        public bool TryMakeCompatible(Atoms? atoms, out Atoms? result) {
            result = MakeCompatible(atoms);
            return result is not null;
        }
        public Atoms? MakeCompatible(Atoms? atoms) {
            if (atoms is null) return null;
            if (IsCompatible(atoms)) return atoms;
            if (IsCreatableFrom(atoms)) return CreateFrom(atoms);
            Atoms last_ditch = new() { Value = atoms.SourceValue, Processor = this };
            return null;
        }
        public virtual ICollection<string> Atoms { get; } = new Collection<string>() { NormalValue };
        public virtual ICollection<string> ValueAtoms { get => Atoms; }
        public virtual float DefaultConfidence { get; } = Confidence.Normal;
        public virtual IDictionary<string, string?> Atomize(string source_value) {
            string working_value = PrepareValue(source_value);
            IDictionary<string, string?> atoms = DefaultAtoms;
            atoms[NormalValue] = FormatValue(working_value);
            return atoms;
        }
        public virtual string? FormatValue(IDictionary<string, string?> value, string? format = null) {
            if (format is null) {
                return value is not null ? JsonSerializer.Serialize(value) : null;
            }
            return format.Format(value);
        }
        public virtual string? FormatValue(string source_value) => source_value.Trim();
        public virtual string? GetAtom(string source_value, string? key = null) {
            key ??= NormalValue;
            if (key.Equals(NormalValue)) return FormatValue(source_value);
            return null;
        }
        public virtual Score? Score(string? source_value) {
            if (string.IsNullOrEmpty(source_value)) return null;
            string normal = FormatValue(source_value) ?? string.Empty;
            if (normal.Length == 0) return null;
            float score = normal.Length / source_value.Length;
            return new() {
                Processor = this,
                Confidence = DefaultConfidence,
                Value = score
            };
        }
        public virtual int Compare(Atoms? left, Atoms? right) {
            IDictionary<string, string?>? left_values = MakeCompatible(left)?.Data;
            IDictionary<string, string?>? right_values = MakeCompatible(right)?.Data;
            return Compare(left_values, right_values);
        }
        public virtual int Compare(IDictionary<string, string?>? left, IDictionary<string, string?>? right) {
            foreach (string name in ValueAtoms) {
                string left_value = left.ValueAs<string>(name) ?? string.Empty;
                string right_value = right.ValueAs<string>(name) ?? string.Empty;
                int result = string.Compare(left_value, right_value);
                if (result != 0) return result;
            }
            return 0;
            // -1 left < right 
            // 0 left == right
            // 1 left > right
        }
        /*public virtual Atoms? AsTypeWith(Atoms values, params string[] requested_atoms) {
            if (values.Keys.ContainsAll(requested_atoms)) return values;
            List<Processor> potential_processors = Instances.Values.Where(item => item.Atoms.ContainsAll(requested_atoms)).ToList();
            potential_processors = potential_processors.OrderByDescending(item=>item.DefaultConfidence).ToList();
            if(ConvertsTo().Intersect(potential_processors) is IEnumerable<Processor> local_convertable) {
                foreach(Processor canidate_type in local_convertable) {
                    if (ConvertTo(canidate_type, values) is Atoms result) return result;
                }
            }
            foreach(Processor canidate_type in potential_processors) {

            }

            foreach(Processor canidate_type in potential_processors) {

            }
            List<Processor> canidates =
                ConvertsTo().Union(values.Processor.ConvertsTo()).Distinct()
                .Where(item => item.Atoms.ContainsAll(requested_atoms))
                .ToList();

            return null;
        }*/

        public virtual string PrepareValue(string value) => value;

    }
}
