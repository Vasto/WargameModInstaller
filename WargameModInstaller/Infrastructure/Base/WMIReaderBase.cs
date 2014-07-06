using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Model;

namespace WargameModInstaller.Infrastructure
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TQuerySource"></typeparam>
    /// <typeparam name="TQuaryResult"></typeparam>
    public abstract class WMIReaderBase<TQuerySource, TQuaryResult>
    {
        private Dictionary<WMIEntryType, Func<TQuerySource, TQuaryResult>> readingQueries;

        protected Dictionary<WMIEntryType, Func<TQuerySource, TQuaryResult>> ReadingQueries
        {
            get
            {
                if (readingQueries == null)
                {
                    readingQueries = CreateReadingQueries();
                }

                return readingQueries;
            }
        }

        protected abstract Dictionary<WMIEntryType, Func<TQuerySource, TQuaryResult>> CreateReadingQueries();
    }

}
