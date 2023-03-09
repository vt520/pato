namespace Pato.Processors {
    public class NumberProcessor : PatternProcessor {
        public override float DefaultConfidence => Confidence.High;
        protected NumberProcessor(Pattern pattern) : base(pattern) { }
        public override ICollection<string> ValueAtoms => new List<string> { "number" };
        public override int Compare(IDictionary<string, string?>? left, IDictionary<string, string?>? right) {
            float left_value = left.ValueAs<float>("number");
            float right_value = right.ValueAs<float>("number");
            if (left_value < right_value) return -1;
            if (left_value > right_value) return 1;
            return 0;
        }
        public override IDictionary<string, string?> NormalizeValues(IDictionary<string, string?> values) {
            values["number"] = values.ValueAs<float>("number").ToString();
            values["decimal"] = values.ValueAs<int>("decimal").ToString();
            values["integer"] = values.ValueAs<int>("integer").ToString();
            return base.NormalizeValues(values);
        }
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
