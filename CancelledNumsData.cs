using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORMAPCancelledNumbers
{
    internal class CancelledNumsData
    {
        public string MapNumber { get; set; }
        public string Taxlot { get; set; }
        public int SortOrder { get; set; }
        public int ObjectId { get; set; }
    }
}
