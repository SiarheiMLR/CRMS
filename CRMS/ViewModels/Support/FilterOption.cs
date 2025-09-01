using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.ViewModels.Support
{
    public class FilterOption<T> where T : struct
    {
        public T? Value { get; }
        public string DisplayName { get; }

        public FilterOption(T? value, string displayName)
        {
            Value = value;
            DisplayName = displayName;
        }

        public override string ToString() => DisplayName;
    }
}
