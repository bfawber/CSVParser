using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CSV
{
	/// <summary>
	/// A generic CSV parser. Still needs to handle qualifiers...
	/// </summary>
    public class CSVParser
    {

	#region Public Methods

		/// <summary>
		/// Parses the CSV file stored in _filePath
		/// </summary>
		/// <typeparam name="T">The type of object the data should be deserialized as</typeparam>
		/// <returns>A List of object T</returns>
		public List<T> Parse<T>(string csvString, string separator = "\n") where T : class
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

			//TODO: create a "more intelligent" line parser
			string[] linesSeparatedBySeparator = csvString.Split(new string[] { separator }, StringSplitOptions.None).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
			
			MethodInfo getGenericValueMethod = typeof(CSVParser).GetMethod(nameof(GetParsedValue), BindingFlags.NonPublic | BindingFlags.Instance);
			Dictionary<Type, MethodInfo> methodCache = new Dictionary<Type, MethodInfo>();

			foreach (string line in linesSeparatedBySeparator)
			{
				string[] lineData = line.Split(',');
				T instance = (T)Activator.CreateInstance(typeOfT);
				for(int i = 0; i < lineData.Length && i < orderMap.Count; i++)
				{
					try
					{
						MethodInfo getSpecificValueMethod;
						if (methodCache.TryGetValue(orderMap[i].Type, out getSpecificValueMethod))
						{
							typeOfT.GetProperty(orderMap[i].Name).SetValue(instance, getSpecificValueMethod.Invoke(this, new[] { lineData[i] }));
						}
						else
						{
							getSpecificValueMethod = getGenericValueMethod.MakeGenericMethod(typeOfT.GetProperty(orderMap[i].Name).PropertyType);
							typeOfT.GetProperty(orderMap[i].Name).SetValue(instance, getSpecificValueMethod.Invoke(this, new[] { lineData[i] }));
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
