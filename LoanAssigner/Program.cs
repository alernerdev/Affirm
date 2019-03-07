using System;
using System.Collections.Generic;
using System.Text;

namespace LoanAssigner
{
	class Program
	{
		static void Main(string[] args)
		{
			LoanManager mgr = new LoanManager("banks.csv", "facilities.csv", "covenants.csv");
			Console.WriteLine("hit Return to finish....");
			Console.ReadLine();

		}
	}
}

