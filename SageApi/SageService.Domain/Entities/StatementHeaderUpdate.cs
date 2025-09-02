using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SageService.Domain.Entities
{
    /// <summary>
    ///  Update object for statement header aggregates
    /// </summary>
    public sealed class StatementHeaderUpdate
    {
        public decimal TotalDue { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Days30 { get; set; }
        public decimal Days60 { get; set; }
        public decimal Days90 { get; set; }
        public decimal Days120Plus { get; set; }
        public string RowHash { get; set; } = string.Empty;
    }
}
