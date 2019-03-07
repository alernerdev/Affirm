using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using LumenWorks.Framework.IO.Csv;


namespace LoanAssigner
{
	class Bank
	{
		public int Id { get; set; }
		public string Name { get; set; }

		public IDictionary<int, Facility> m_facilities;
		public IList<Covenant> m_bankRules;
	}

	class BankReader
	{
		public BankReader()
		{
		}

		public IDictionary<int, Bank> Read(StreamReader reader)
		{
			//holds the property mappings
			Dictionary<string, int> map = new Dictionary<string, int>();

			IDictionary<int, Bank> banks = new Dictionary<int, Bank>();

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
					Bank bank = new Bank();

					bank.Id = Convert.ToInt32(csv[map["id"]]);
					bank.Name = csv[map["name"]];

					banks.Add(bank.Id, bank);
				}
			}

			return banks;
		}
	}
}
