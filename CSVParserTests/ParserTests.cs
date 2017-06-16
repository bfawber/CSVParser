using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSV;
using System.Collections.Generic;

namespace CSVParserTests
{
    [TestClass]
    public class ParserTests
    {

		#region Base Case

		[TestMethod]
        public void CanCreateParser()
        {
			new CSVParser();
        }

		#endregion

		#region Zero Case

		[TestMethod]
		public void ParseCanHandleEmptyStrings()
		{
			CSVParser parser = new CSVParser();

			Assert.ThrowsException<CSVParserException>(() => parser.Parse<SimpleObject>("1,2,three,four--3,4,five,six", string.Empty));
			Assert.ThrowsException<CSVParserException>(() => parser.Parse<SimpleObject>(string.Empty, string.Empty));
			Assert.IsNotNull(parser.Parse<SimpleObject>(string.Empty));
		}

		[TestMethod]
		public void ParseCanHandleNulls()
		{
			CSVParser parser = new CSVParser();

			Assert.IsNotNull(parser.Parse<SimpleObject>(null));
			Assert.ThrowsException<CSVParserException>(() => parser.Parse<SimpleObject>(null, null));
			Assert.ThrowsException<CSVParserException>(() => parser.Parse<SimpleObject>(@"1,2,three,four--3,4,five,six", null));
		}

		#endregion

		#region Happy Path

		[TestMethod]
		public void CanParseSimpleObjectDoubleDash()
		{
			const string simpleCSV = @"1,2,three,four--3,4,five,six";
			CSVParser parser = new CSVParser();
			List<SimpleObject> result = parser.Parse<SimpleObject>(simpleCSV, "--");

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Count == 2);
		}

		[TestMethod]
		public void CanParseSimpleObjectNewLine()
		{

			const string simpleCSV = "1,2,three,four\n3,4,five,six";
			CSVParser parser = new CSVParser();
			List<SimpleObject> result = parser.Parse<SimpleObject>(simpleCSV, "\n");

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Count == 2);
		}

		#endregion

		#region Edge Case

		public void CanParseSimpleObject_WithNoSeparatorInString()
		{
			const string simpleCSV = @"1,2,three,four";
			CSVParser parser = new CSVParser();
			List<SimpleObject> result = parser.Parse<SimpleObject>(simpleCSV, "--");

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Count == 1);
		}

		[TestMethod]
		public void ParseCanHandleValidEmptyData()
		{
			const string simpleCSV = @"1,2,,";
			CSVParser parser = new CSVParser();
			List<SimpleObject> result = parser.Parse<SimpleObject>(simpleCSV, "--");

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Count == 1);
		}

		[TestMethod]
		public void ParseCanHandleClassWithOnlySomeAttributes()
		{
			const string simpleCSV = @"1,three--3,six";
			CSVParser parser = new CSVParser();
			List<SimpleObject2> result = parser.Parse<SimpleObject2>(simpleCSV, "--");

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Count == 2);
		}

		[TestMethod]
		public void ParseCanHandleClassWithNoAttributes()
		{
			const string simpleCSV = @"1,2,three,four--3,4,five,six";
			CSVParser parser = new CSVParser();
			List<SimpleObject3> result = parser.Parse<SimpleObject3>(simpleCSV, "--");

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Count == 0);
		}

		[TestMethod]
		public void ParseCanHandleStringWithTrailingSeparator()
		{
			const string simpleCSV = @"1,2,three,four--3,4,five,six--";
			CSVParser parser = new CSVParser();
			List<SimpleObject> result = parser.Parse<SimpleObject>(simpleCSV, "--");

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Count == 2);
		}

		[TestMethod]
		public void ParseCanHandleStringWithRandomNewlines()
		{
			const string simpleCSV = "1,2,three\n,four--3\n,4,five,six";
			CSVParser parser = new CSVParser();
			List<SimpleObject> result = parser.Parse<SimpleObject>(simpleCSV, "--");

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Count == 2);
		}

		#endregion

		#region Negative Case

		[TestMethod]
		public void ParseThrowsExceptionOnBadData()
		{
			const string simpleCSV1 = @"+dfadfa&d,fs++afda,ddddd";

			CSVParser parser = new CSVParser();
			Assert.ThrowsException<CSVParserException>(() => parser.Parse<SimpleObject>(simpleCSV1, "--"));
		}

		#endregion

		#region Nested Test Classes

		public class SimpleObject
		{
			[CSVOrder(0)]
			public int ItemOne { get; set; }

			[CSVOrder(1)]
			public int ItemTwo { get; set; }

			[CSVOrder(2)]
			public string ItemThree { get; set; }

			[CSVOrder(3)]
			public string ItemFour { get; set; }
		}

		public class SimpleObject2
		{
			[CSVOrder(0)]
			public int ItemOne { get; set; }
			
			public int ItemTwo { get; set; }

			[CSVOrder(1)]
			public string ItemThree { get; set; }
			
			public string ItemFour { get; set; }
		}


		public class SimpleObject3
		{
			public int ItemOne { get; set; }

			public int ItemTwo { get; set; }
			
			public string ItemThree { get; set; }

			public string ItemFour { get; set; }
		}

		#endregion
	}
}
