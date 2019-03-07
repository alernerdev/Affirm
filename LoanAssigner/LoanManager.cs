using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LoanAssigner
{
	class LoanManager
	{
		EnvironmentManager m_envMgr;
		IDictionary<int, Bank> m_banks;
		IDictionary<int, double> m_yields;

		public LoanManager(string banksFilename, string facilityFilename, string covenantsFilename)
		{
			// one time initialization of partners/rules available
			m_envMgr = new EnvironmentManager(banksFilename, facilityFilename, covenantsFilename);
			m_banks = m_envMgr.Banks;

			// initialize all expected yields per facility to 0
			// assuming here that facility ids are unique across banks ??
			m_yields = new Dictionary<int, double>();
			foreach (Bank bank in m_banks.Values)
				foreach (Facility facility in bank.m_facilities.Values)
					m_yields[facility.FacilityId] = 0;

			// for the sake of the exercise, lets pretend that events are fired as loans are arriving on some feed
			m_envMgr.LoanRequestArrived += this.CallbackLoanRequestArrived;

			m_envMgr.Dump();

			m_envMgr.LoadLoans("loans.csv");

			// "stream" of loans is done
			foreach (KeyValuePair<int, double> kvp in m_yields)
				SaveToYields("yields.csv", kvp.Key, kvp.Value);

		}

		void CallbackLoanRequestArrived(object sender, Loan loan)
		{
			// for each incoming loan, loop through each facility and find max expected_yield among them
			// ....while respecting a list of covenants for each facility

			int bestBankId = -1;
			int bestFacilityId = -1;
			double highestYield = 0;

			foreach (Bank bank in m_banks.Values)
			{
				// pick the lowest hanging fruit -- do the bank global rules first
				if (bank.m_bankRules != null)
				{
					foreach (Covenant cov in bank.m_bankRules)
						if (!Validate(loan, cov))
							// this loan failed this bank's global rules.  Move on
							continue;
				}

				// if got to this point, either global rules were passed or there werent any

				// lets check rules from each facility from this bank
				foreach (Facility facility in bank.m_facilities.Values)
				{
					foreach (Covenant cov in facility.m_rules)
					{
						if (!Validate(loan, cov))
							// this loan failed this facility's rules.  Move on
							continue;
					}

					// can this facilility handle the size of the loan ?
					if (!ValidateCapacity(loan, facility))
					if (loan.Amount > facility.Capacity)
					{
						continue;
					}

					// if got to this point, we have a facility candidate for a loan

					double expectedYield = CalcYield(loan, facility);
					if (expectedYield > highestYield)
					{
						// save the  winning values
						bestBankId = bank.Id;
						bestFacilityId = facility.FacilityId;
						highestYield = expectedYield;
					}		
				}
			} // end of banks loop

			// accumulate this all day, in theory
			m_yields[bestFacilityId] += highestYield;

			SaveToAssignments("assignments.csv", loan.LoanId, bestFacilityId);

			Console.WriteLine(string.Format("for loan {0} best facility is {1} at bank {2}", loan.LoanId, bestFacilityId, bestBankId));
		}

		bool ValidateCapacity(Loan loan, Facility facility)
		{
			// TODO
			// I am not sure if this is sufficient or I am supposed to keep a running total of the loans issued per facility
			if (loan.Amount > facility.Capacity)
				return true;
			else
				return false;
		}

		protected void SaveToAssignments(string filename, int loanId, int facilityId)
		{
			bool fileExists = false;
			if (File.Exists(filename))
				fileExists = true;

			using (StreamWriter w = new StreamWriter(filename, true))
			{
				if (!fileExists)
					w.WriteLine("loan_id,facility_id");

				w.WriteLine(string.Format("{0},{1}", loanId, facilityId));
			}
		}

		protected void SaveToYields(string filename, int facilityId, double yield)
		{
			bool fileExists = false;
			if (File.Exists(filename))
				fileExists = true;

			using (StreamWriter w = new StreamWriter(filename, true))
			{
				if (!fileExists)
					w.WriteLine("facility_id,expected_yield");

				w.WriteLine(string.Format("{0},{1}", facilityId,yield));
			}
		}

		bool Validate(Loan loan, Covenant covenant)
		{
			if (loan.State == covenant.bannedState)
				return false;

			if (covenant.maxDefaultLikely != 0)
			{
				if (loan.DefaultLikely > covenant.maxDefaultLikely)
					return false;
			}

			return true;
		}

		double CalcYield(Loan loan, Facility facility)
		{
			double gross = (1 - loan.DefaultLikely) * loan.InterestRate * loan.Amount;
			double cost = loan.DefaultLikely * loan.Amount + facility.InterestRate * loan.Amount;
			double expected = gross - cost;

			return expected;
		}
	}
}