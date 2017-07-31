using System.Collections.Generic;

namespace Cllearworks.COH.Utility
{
    public class PagedList<T> : List<T>
    {
        public List<T> Records { get; set; }
        public int TotalRecords { get; set; }
    }
}
