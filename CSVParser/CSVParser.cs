using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CSV
{
	/// <summary>
	/// A generic CSV parser.
	/// </summary>
    public class CSVParser
    {

		#region Constants

		private const Char CommaToken = ',';

	#endregion

	#region Public Methods

		/// <summary>
		/// Parses the CSV file stored in _filePath
		/// </summary>
		/// <typeparam name="T">The type of object the data should be deserialized as</typeparam>
		/// <returns>A List of object T</returns>
		public List<T> Parse<T>(string csvString, string separator = "\n", char qualifier = '"') where T : class
		{
			if(string.IsNullOrEmpty(separator))
			{
				throw new CSVParserException("Separator cannot be null or empty. Pass in a valid argument, or don't pass in any argument for the \\n character");
			}

			List<T> result = new List<T>();
			if(string.IsNullOrEmpty(csvString))
			{
				return result;
			}

			Type typeOfT = typeof(T);

			// Get the map of what order the data appears to what property it corresponds to on the desired storage object
			Dictionary<int, NameType> orderMap = GetOrderMap(typeOfT);
			if(orderMap.Count < 1)
			{
				return result;
			}
			
			List<List<string>>linesSeparatedBySeparator = GetParsedLines(csvString, separator, qualifier);

			MethodInfo getGenericValueMethod = typeof(CSVParser).GetMethod(nameof(GetParsedValue), BindingFlags.NonPublic | BindingFlags.Instance);
			Dictionary<Type, MethodInfo> methodCache = new Dictionary<Type, MethodInfo>();

			foreach (List<string> line in linesSeparatedBySeparator)
			{
				T instance = (T)Activator.CreateInstance(typeOfT);
				
				for(int i = 0; i < line.Count && i < orderMap.Count; i++)
				{
					try
					{
						MethodInfo getSpecificValueMethod;
						if (methodCache.TryGetValue(orderMap[i].Type, out getSpecificValueMethod))
						{
							typeOfT.GetProperty(orderMap[i].Name).SetValue(instance, getSpecificValueMethod.Invoke(this, new[] { line[i] }));
						}
						else
						{
							getSpecificValueMethod = getGenericValueMethod.MakeGenericMethod(typeOfT.GetProperty(orderMap[i].Name).PropertyType);
							typeOfT.GetProperty(orderMap[i].Name).SetValue(instance, getSpecificValueMethod.Invoke(this, new[] { line[i] }));
						}
					}
					catch(Exception ex)
					{
						throw new CSVParserException(ex.Message);
					}
				}

				result.Add(instance);
			}

			return result;
		}

	#endregion

	#region Private Methods

		private List<List<string>> GetParsedLines(string csvString, string separator, char qualifier)
		{
			List<List<string>> lines = new List<List<string>>();
			StringBuilder bob = new StringBuilder();
			bool currentlyQualified = false;
			List<string> lineData = new List<string>();

			for (int i = 0; i < csvString.Length; i++)
			{
				if (csvString[i] == qualifier)
				{
					currentlyQualified = !currentlyQualified;
				}
				else if (!currentlyQualified && csvString[i] == separator[0])
				{
					bool shouldSplit = true;
					for (int j = i; j < i + separator.Length; j++)
					{
						if (csvString[j] != separator[j - i])
						{
							shouldSplit = false;
							break;
						}
					}

					if (shouldSplit)
					{
						if (bob.Length > 0)
						{
							lineData.Add(bob.ToString());
						}
						lines.Add(lineData);
						lineData = new List<string>();
						bob.Clear();
						i += separator.Length - 1;
					}
				}
				else if (!currentlyQualified && csvString[i] == CommaToken)
				{
					lineData.Add(bob.ToString());
					bob.Clear();
				}
				else
				{
					bob.Append(csvString[i]);
				}
			}

			if (currentlyQualified)
			{
				throw new CSVParserException("Qualifier never closed, invalid CSV!");
			}

			if(bob.Length > 0)
			{
				lineData.Add(bob.ToString());
			}
			if (lineData.Any())
			{
				lines.Add(lineData);
			}

			return lines;
		}

		private T GetParsedValue<T>(string data)
		{
			try
			{
				return (T)Convert.ChangeType(data, typeof(T));
			}
			catch(Exception ex)
			{
				throw new CSVParserException($"Could not convert data attribute to desired type {typeof(T).Name}.\nOuter Exception: {ex.Message}");
			}
		}

		private Dictionary<int, NameType> GetOrderMap(Type modelType)
		{
			Dictionary<int, NameType> orderMap = new Dictionary<int, NameType>();

			foreach (PropertyInfo prop in modelType.GetProperties())
			{
				CSVOrderAttribute orderAttribute = prop.GetCustomAttribute<CSVOrderAttribute>();

				if (orderAttribute != null)
				{
					NameType propNameType = new NameType(prop.Name, prop.PropertyType);
					orderMap.Add(orderAttribute.Order, propNameType);
				}
			}

			return orderMap;
		}

	#endregion

	#region Nested Classes

		public class NameType
		{
			public readonly string Name;

			public readonly Type Type;

			public NameType(string name, Type type)
			{
				Name = name;
				Type = type;
			}

			#region Equality and Hashcode

			public override bool Equals(object obj)
			{
				return Equals(obj as NameType);
			}

			public bool Equals(NameType other)
			{
				return ReferenceEquals(this, other) ||
					other != null
					&& this.Name == other.Name
					&& this.Type == other.Type;
			}

			public static bool operator==(NameType lhs, NameType rhs)
			{
				if (ReferenceEquals(lhs, rhs)) return true;
				if (lhs == null || rhs == null) return false;
				return lhs.Equals(rhs);
			}

			public static bool operator!=(NameType lhs, NameType rhs)
			{
				return !(lhs == rhs);
			}

			public override int GetHashCode()
			{
				return Name.GetHashCode() +
					Type.GetHashCode();
			}

			#endregion
		}
    }

	#endregion
}
