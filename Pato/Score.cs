namespace Pato {
    /// <summary>
    /// Represents the quality of a match between a processor and a given input
    /// </summary>
    public struct Score {
        /// <summary>
        /// A prettyprint operator, purely for easy
        /// </summary>
        /// <param name="score">The core object to prettyprint</param>
        public static implicit operator string(Score score) => $"{score.Processor.GetType().Name}: {score.Value} ({score.Confidence})";
        /// <summary>
        /// Used for ordering and comparisons
        /// </summary>
        /// <param name="score">The Score object to convert to a float</param>
        public static implicit operator float(Score score) => score.Value * score.Confidence;
        /// <summary>
        /// Initializes a Score object for a given Processor
        /// </summary>
        /// <param name="processor">The Processor this Score applies to</param>
        /// <param name="value">The raw match value</param>
        /// <param name="confidence">The confidence the processor has in it's answer</param>
        public Score(Processor? processor = null, float value = 0, float confidence = 0) {
            Processor = processor ?? Processor.Default;
            Value = value;
            Confidence = confidence;
        }
        /// <summary>
        /// A value between 0 and 1 that indicates how confident the processor is in its acceptance of a value
        /// 0 indicates that the processor can not find any meaningful information in the given string
        /// 1 indicates an absolute certainty that the given data is something the processor is meant to
        ///   process
        /// </summary>
        public float Confidence;
        /// <summary>
        /// This value indicates the percentage of a given input determined to be meaningful by the processor
        /// ex: Processor A considers [A-Za-z] and ignores everything else
        ///     "     Hello     " = (Meaningful Count) / (Total Count)  = 5 / 15 == .3333~
        /// </summary>
        public float Value;
        /// <summary>
        /// The Processor that this Score is associated with
        /// </summary>
        public Processor Processor;
    }
}
