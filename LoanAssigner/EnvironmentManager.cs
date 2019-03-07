using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using LumenWorks.Framework.IO.Csv;

namespace LoanAssigner
{
	class EnvironmentManager
	{
		IDictionary<int, Bank> m_banks;
		Dictionary<int, List<Covenant>> m_bankCovenants;

		public EnvironmentManager(string banksFilename, string facilityFilename, string covenantsFilename)
		{
			BankReader bankReader = new BankReader();
			m_banks = bankReader.Read(new StreamReader(banksFilename));

			FacilityReader facilityReader = new FacilityReader();
			facilityReader.Read(new StreamReader(facilityFilename), m_banks);

			CovenantReader covenantReader = new CovenantReader();
			covenantReader.Read(new StreamReader(covenantsFilename), m_banks);
		}

		public IDictionary<int, Bank> Banks
		{
			get { return m_banks; }
		}

		// write everything out
		public void Dump()
		{
			foreach (Bank bank in m_banks.Values)
			{
				Console.WriteLine(string.Format("bank {0} {1} -- {2}", bank.Id, bank.Name, bank.m_bankRules == null ? "no bank level rules" : "bank rules!"));
				foreach (Facility facility in bank.m_facilities.Values)
				{
					Console.WriteLine(string.Format("\tbank {0} facility {1} capacity {2}", bank.Id, facility.FacilityId, facility.Capacity));

					foreach (Covenant cov in facility.m_rules)
					{
						Console.WriteLine
						(
							string.Format
							(
								"\t\tbank {0} facility {1} maxDefault {2} bannedState {3}",
								bank.Id, facility.FacilityId, cov.maxDefaultLikely, cov.bannedState
							)
						);
					}
				}
			}
		}

		public void LoadLoans(string loanFilename)
		{
			//holds the property mappings
			Dictionary<string, int> map = new Dictionary<string, int>();

			// open the file "data.csv" which is a CSV file with headers
			using (CsvReader csv = new CsvReader(new StreamReader(loanFilename), true))
			{
				int fieldCount = csv.FieldCount;
				string[] headers = csv.GetFieldHeaders();

				for (int i = 0; i < fieldCount; i++)
				{
					map[headers[i]] = i; // track the index of each column name
				}

				while (csv.ReadNextRecord())
				{
					Loan loan = new Loan();

					loan.LoanId = Convert.ToInt32(csv[map["id"]]);
					loan.Amount = Convert.ToInt64(csv[map["amount"]]);
					loan.DefaultLikely = Convert.ToDouble(csv[map["default_likelihood"]]);
					loan.InterestRate = Convert.ToDouble(csv[map["interest_rate"]]);
					loan.State = csv[map["state"]];

					Console.WriteLine
					(
						string.Format
						(
							"loan {0} amount {1} defaultLikely {2} state {3}",
							loan.LoanId, loan.Amount, loan.DefaultLikely, loan.State
						)
					);

					// fire event
					OnLoanRequestArrived(loan);
				}
			}
		}

		protected virtual void OnLoanRequestArrived(Loan loan)
		{
			LoanRequestArrivedHandler handler = LoanRequestArrived;
			if (handler != null)
			{
				handler(this, loan);
			}
		}

		public delegate void LoanRequestArrivedHandler(Object o, Loan l);
		public event LoanRequestArrivedHandler LoanRequestArrived;
	}

}
