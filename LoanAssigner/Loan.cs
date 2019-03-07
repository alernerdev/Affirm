using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanAssigner
{
	class Loan
	{
			public int LoanId { get; set; }
			public long Amount { get; set; } // in cents
			public double InterestRate { get; set; }
			public double DefaultLikely { get; set; } //
			public string State { get; set; }
	}
}
