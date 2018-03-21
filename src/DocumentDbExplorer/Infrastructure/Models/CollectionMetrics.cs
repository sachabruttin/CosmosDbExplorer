using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.Infrastructure.Models
{
    public class CollectionMetric
    {
        public int PartitionCount { get; set; }

        public long DocumentCount { get; set; }

        public long DocumentSize { get; set; }

        public List<PartitionKeyRangeStatistics> PartitionMetrics {get; set;}
    }
}
