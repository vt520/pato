namespace Pato.Values {
    public class EmptyValue : Processor {
        protected EmptyValue() : base() { }
        public override float DefaultConfidence => float.MaxValue;
        public override Score? Score(string? source_value) {
            if (string.IsNullOrEmpty(source_value)) return new Score(this) {
                Confidence = Confidence.High,
                Value = Confidence.High,
            };
            return null;
        }
        public override IDictionary<string, string?> Atomize(string source_Value)
            => new Dictionary<string, string?> { { NormalValue, string.Empty } };
        public override string? FormatValue(IDictionary<string, string?> values, string? format = null) => string.Empty;
        public override string? FormatValue(string source_value) => string.Empty;
    }
}
