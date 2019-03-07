using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using LumenWorks.Framework.IO.Csv;


namespace LoanAssigner
{
	class Covenant
	{
		public int bankId { get; set; }
		public int facilityId { get; set; }
		public double maxDefaultLikely { get; set; }
		public string bannedState { get; set; }
	}

	class CovenantReader
	{
		public CovenantReader()
		{
		}

		public void Read(StreamReader reader, IDictionary<int, Bank> banks)
		{
			//holds the property mappings
			Dictionary<string, int> map = new Dictionary<string, int>();

			List<Covenant> covList = new List<Covenant>();

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
					string facilityId = csv[map["facility_id"]];
					int bankId = Convert.ToInt32(csv[map["bank_id"]]);

					Bank bank = banks[bankId];


					// max_default_likelihood and/or bannedState can be present -- at facility OR bank level

					string maxDefault = csv[map["max_default_likelihood"]];
					if (string.IsNullOrEmpty(maxDefault))
					{
						// value not there
					}
					else
					{
						Covenant cov = CreateCovenantHelper(map, csv, banks, facilityId);

						// if value present, likely default is just one covenant
						cov.maxDefaultLikely = Convert.ToDouble(maxDefault);

						// either the rule goes at the bank level, or facility level
						if (string.IsNullOrEmpty(facilityId))
							bank.m_bankRules.Add(cov);
						else
							AddCovenant(bank, cov.facilityId, cov);
					}

					string bannedState = csv[map["banned_state"]];
					if (string.IsNullOrEmpty(bannedState))
					{
						// value not there
					}
					else
					{
						Covenant cov = CreateCovenantHelper(map, csv, banks, facilityId);

						// if value present, state is a separate covenant
						cov.bannedState = bannedState;

						if (string.IsNullOrEmpty(facilityId))
							bank.m_bankRules.Add(cov);
						else
							AddCovenant(bank, cov.facilityId, cov);
					}
				}
			}
		} // end of Read()

		protected Covenant CreateCovenantHelper(Dictionary<string, int> columnMap, CsvReader csv, IDictionary<int, Bank> banks, string facilityId)
		{
			Covenant cov = new Covenant();
			cov.bankId = Convert.ToInt32(csv[columnMap["bank_id"]]);

			Bank bank = banks[cov.bankId];

			// if facilityid is present, it is facility level, otherwise, bank level
			if (string.IsNullOrEmpty(facilityId))
			{
				// global rule for the bank. Is this the very first rule?
				if (bank.m_bankRules == null)
				{
					bank.m_bankRules = new List<Covenant>();
				}
			}
			else
			{
				cov.facilityId = Convert.ToInt32(facilityId);
			}

			return cov;
		}

		protected void AddCovenant(Bank bank, int facilityId, Covenant cov)
		{
			Facility facility = bank.m_facilities[cov.facilityId];
			if (facility.m_rules == null)
				facility.m_rules = new List<Covenant>();

			facility.m_rules.Add(cov);
		}

	}
}
