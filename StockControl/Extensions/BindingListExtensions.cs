using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockControl.Extensions
{
    public static class BindingListExtensions
    {
        public static BindingList<T> ToBindingList<T>(this IEnumerable<T> enumerable)
        {
            return new BindingList<T>(new List<T>(enumerable));
        }
    }
}
