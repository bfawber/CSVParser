using System;

namespace CSV
{

	/// <summary>
	/// An attribute to decorate the Model class with for mapping properties to CSV data
	/// </summary>
    public class CSVOrderAttribute : Attribute
    {
		public int Order { get; set; }

		public CSVOrderAttribute(int order)
		{
			Order = order;
		}
    }
}
