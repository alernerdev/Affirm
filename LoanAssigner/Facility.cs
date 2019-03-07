using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using LumenWorks.Framework.IO.Csv;


namespace LoanAssigner
{
	class Facility
	{
		public int BankId { get; set; }
		public int FacilityId { get; set; }
		public double InterestRate { get; set; } // rate we are charged
		public long Capacity { get; set; } // capacity in cents

		public IList<Covenant> m_rules;
	}

	class FacilityReader
	{
		public FacilityReader()
		{
		}

		public void Read(StreamReader reader, IDictionary<int, Bank> banks)
		{
			//holds the property mappings
			Dictionary<string, int> map = new Dictionary<string, int>();

			// open the file "data.csv" which is a CSV file with headers
			using (CsvReader csv = new CsvReader(reader, true))
			{
				int fieldCount = csv.FieldCount;
				string[] headers = csv.GetFieldHeaders();

				for (int i = 0; i < fieldCount; i++)
				{
					map[headers[i]] = i; // track the index of each column name
				}

				while (csv.ReadNextRecord())
				{
					Facility facility = new Facility();

					facility.BankId = Convert.ToInt32(csv[map["bank_id"]]);
					facility.FacilityId = Convert.ToInt32(csv[map["id"]]);
					facility.Capacity = (long)Convert.ToDouble(csv[map["amount"]]);
					facility.InterestRate = Convert.ToDouble(csv[map["interest_rate"]]);

					Bank bank = banks[facility.BankId];
					if (bank.m_facilities == null)
					{
						// if facilities for this bank dont exist, make one
						bank.m_facilities = new Dictionary<int, Facility>();
					}

					bank.m_facilities.Add(facility.FacilityId, facility);
				}
			}
		}
	}
}
