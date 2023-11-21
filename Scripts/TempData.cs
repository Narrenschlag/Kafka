using System.Collections.Generic;
using Godot;

namespace Kafka
{
	public partial class TempData : Node
	{
		#region Static
		private static Dictionary<string, object> values;
		public static Dictionary<string, object> Values
		{
			get
			{
				if (values == null) values = new Dictionary<string, object>();
				return values;
			}
		}

		public static bool TryGet<T>(string key, out T value) => Singleton.tryGet(key, out value);
		public static T Get<T>(string key, T @default) => Singleton.get(key, @default);

		public static bool TryAdd<T>(string key, T value) => Singleton.tryAdd(key, value);
		public static void Set<T>(string key, T value) => Singleton.set(key, value);

		public static bool HasValue(string key) => Singleton.hasValue(key);
		public static void Remove(string key) => Singleton.remove(key);

		#endregion

		#region Local
		private static TempData Singleton;
		public override void _EnterTree()
		{
			Singleton = this;
		}

		public virtual T get<T>(string key, T @default) => tryGet(key, out T value) ? value : @default;
		public virtual bool tryGet<T>(string key, out T value)
		{
			if (Values.TryGetValue(key, out object result))
			{
				value = (T)result;
				return true;
			}

			value = default;
			return false;
		}

		public virtual void set<T>(string key, T value)
		{
			if (!tryAdd(key, value)) Values[key.Trim()] = value;
		}

		public virtual bool tryAdd<T>(string key, T value)
		{
			if (hasValue(key)) return false;

			Values.Add(key.Trim(), value);
			return true;
		}

		public virtual bool hasValue(string key) => Values.ContainsKey(key.Trim());
		public virtual void remove(string key)
		{
			if (hasValue(key)) Values.Remove(key.Trim());
		}
		#endregion
	}
}