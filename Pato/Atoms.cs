using System;
using System.Runtime;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;

namespace Pato {
    public class Atoms {
        public static implicit operator Dictionary<string, string?>?(Atoms atoms) => atoms.Data as Dictionary<string, string?>;
        private IDictionary<string, string?>? _Data = null;
        public ICollection<string> ValueAtoms => Processor.ValueAtoms;
        public ICollection<string> CurrentAtoms => Data.Keys.ToList();
        public ICollection<string> AvailableAtoms => CurrentAtoms;

        private int DictionaryHash = 0;
        public bool CacheValid { get; internal set; } = false;
        private int DataHash = 0;
        public IDictionary<string, string?> Data {
            get {
                _Data ??= new Dictionary<string, string?>();
                if (!CacheValid) {
                    foreach ((string name, string? value) in Processor.Atomize(SourceValue)) {
                        if (!_Data.ContainsKey(name)) {
                            _Data.Add(name, value);
                        } else {
                            _Data[name] = value;
                        }
                    }
                    DataHash = _Data.GetValueHashCode(ValueAtoms);// .ComputeValueHash();
                } else if (DataHash != _Data.GetValueHashCode(ValueAtoms)) {
                    _Data = Processor.Atomize(Processor.FormatValue(_Data) ?? string.Empty);
                    DataHash = _Data.GetValueHashCode(ValueAtoms);
                }
                CacheValid = true;
                return _Data;
            }
            set {
                if (Processor.CanCreateFromDictionary(value)) {
                    _Data = value;
                    CacheValid = true;
                    SourceValue = Processor.FormatValue(value) ?? string.Empty;
                }
            }
        }
        public bool IsValid => Processor.CanCreateFromDictionary(_Data);
        public static implicit operator string?(Atoms value) => value.Value;
        public static explicit operator Atoms(string value) => new() { Value = value };
        public Processor? _Processor;
        public Processor Processor {
            get => _Processor ??= Processor.ProcessorFor(SourceValue);
            set {
                CacheValid = false;
                _Processor = value;
            }
        }
        public string SourceValue { get; private set; } = string.Empty;
        public string? Value {
            get => Processor.FormatValue(Data);
            set {
                CacheValid = false;
                SourceValue = value ?? string.Empty;
            }
        }
        public string? ValueOf(string name) {
            if (Data.TryGetValue(name, out string? value)) return value;
            // advanced resolver hookeyjoo
            return null;
        }
        public int Compare(Atoms atom) {
            return Processor.Compare(this, atom);
        }
    }
}
