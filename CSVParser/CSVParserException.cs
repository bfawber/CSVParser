using System;

namespace CSV
{
    public class CSVParserException : Exception
    {
		public CSVParserException(string message) : base(message)
		{ }
    }
}
