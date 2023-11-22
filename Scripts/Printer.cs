using System.Collections.Generic;
using Godot;

using Cutulu;

namespace Kafka
{
	public partial class Printer : Node
	{
		[Export] private PrintMode Output = PrintMode.Direct;
		[Export] private bool IgnoreCommands = false;

		[ExportGroup("Direct")]
		[Export] private Label TitleMesh;
		[Export] private Label ContentMesh;

		[Signal] public delegate void OnPrintEventHandler(string title, string content, string[] options);
		[Signal] public delegate void OnSetActiveEventHandler(bool active);

		[Signal] public delegate void StringCmdEventHandler(string cmd, string[] args);
		public StringCmdEventHandler onCmd;

		public void Print(KNode node)
		{
			if (node.IsNull()) return;

			// Enrich texts
			(string cmd, string[] args, bool local)[] cmds = ExtractCmds(node.Statement.Text, out string content);
			string title = EnrichString(node.Statement.Name);

			// Enrich the options
			string[] options = node._options.IsEmpty() ? null : new string[node._options.Length];
			if (options.NotEmpty())
				for (int i = 0; i < options.Length; i++)
				{
					ExtractCmds(node._options[i].Value, out string optionText);
					options[i] = EnrichString(optionText);
				}

			// Execute commands first
			if (!IgnoreCommands) ExecuteStringCmds(cmds);

			// Debugging
			if (Output == PrintMode.DebugConsole)
			{
				$"{title}> {content}".Log();

				if (node._options.NotEmpty())
					for (int i = 0; i < node._options.Length; i++)
						$"  > '{options[i]}' [{node._options[i].Key}]".Log();
				else $"  >[{node.next.Key}]".Log();

				if (cmds.NotEmpty())
				{
					string dbgStr = "  >{";
					for (int i = 0; i < cmds.Length; i++)
					{
						if (i > 0) dbgStr += ", ";
						dbgStr += cmds[i].cmd + $"({(cmds[i].local ? 'L' : 'G')})";
					}
					(dbgStr + "}").Log();
				}
				else "  >{}".Log();

				return;
			}

			// Sending Signals
			if (Output != PrintMode.Direct) EmitSignal("OnPrint", title, content, options);
			if (Output == PrintMode.Signal) return;

			// Update Self
			UpdateSelf(title, content, options);
		}

		protected virtual void UpdateSelf(string title, string content, string[] options)
		{
			// Assigning text mesh values
			ContentMesh.Text = content;
			TitleMesh.Text = title;
		}

		public void ExecuteStringCmds(string plainText)
		{
			(string cmd, string[] args, bool local)[] cmds = ExtractCmds(plainText, out string message);
			if (cmds.NotEmpty()) ExecuteStringCmds(cmds);
		}

		public void ExecuteStringCmds((string cmd, string[] args, bool local)[] cmds)
		{
			Node localNode = Narrator.GetLocalNode();
			bool canLocal = localNode.NotNull();

			if (cmds.NotEmpty())
				foreach ((string cmd, string[] args, bool local) in cmds)
				{
					// Local commands
					if (local)
					{
						if (canLocal)
							if (args.NotEmpty()) localNode.Call(cmd, args);
							else localNode.Call(cmd);
						continue;
					}

					// Global commands
					if (!BuiltInCmd(cmd, args))
					{
						if (onCmd != null) onCmd(cmd, args);

						if (args.NotEmpty()) EmitSignal("StringCmd", cmd, args);
						else EmitSignal("StringCmd", cmd);
					}
				}
		}

		public void SetActive(bool active)
		{
			// Set active
			EmitSignal("OnSetActive", active);
			Core.SetActive(this, active);
		}

		// Comment order reflects the order of function calls
		#region NestedString Utilized
		// {local:turnAround()}
		// {global:saveValue(1)}
		#region String Command Utility
		private const char SEP_CHAR = ',';
		private const char OPN_CHAR = '(';
		private const char CLS_CHAR = ')';

		private static (string cmd, string[] args, bool local)[] ExtractCmds(string plainText, out string message)
		{
			NestedString nestedString = NestedString.Parse(plainText, new string[2] { "{global", "{local" }, "", ':', '}');
			message = nestedString.Value;

			// List of results
			List<(string cmd, string[] args, bool local)> result = new List<(string cmd, string[] args, bool local)>();

			// Local functions
			read(nestedString.ReadValues("{local"), true);

			// Global functions
			read(nestedString.ReadValues("{global"), false);

			// Shortcut function
			void read(string[] lines, bool local)
			{
				if (lines.IsEmpty()) return;

				foreach (string line in lines)
				{
					string cmd = line.Extract(new List<char>() { OPN_CHAR, CLS_CHAR, SEP_CHAR }, out string[] args).Trim();
					if (cmd.IsEmpty()) continue;

					if (args.NotEmpty())
						for (int i = 0; i < args.Length; i++)
							args[i] = args[i].Trim();

					else args = null;

					result.Add((cmd, args, local));
				}
			}

			return result.IsEmpty() ? null : result.ToArray();
		}
		#endregion

		// {set:integer:playerHealth=10}
		// {mod:integer:playerHealth=-2}
		// {get:string:playername}
		#region Text Enrichment
		public static string EnrichString(string plainText)
		{
			NestedString nestedString = NestedString.Parse(plainText, new string[3] { "{get", "{mod", "{set" }, "", ':', '}');
			int removed = 0;

			// Set values
			(string value, int start, int last)[] lines = nestedString.ReadValues2("{set");
			if (lines.NotEmpty())
				foreach ((string value, int start, int last) in lines)
				{
					if (!parse(value, ':', out string type, out string valueSet)) continue;
					if (!parse(valueSet, '=', out string valueName, out string newValue)) continue;

					switch (type.ToLower()[0])
					{
						case 'i':
							TempData.Set(valueName, int.Parse(newValue));
							break;

						case 'f':
							TempData.Set(valueName, float.Parse(newValue));
							break;

						case 'd':
							TempData.Set(valueName, double.Parse(newValue));
							break;

						case 'c':
							TempData.Set(valueName, newValue[0]);
							break;

						case 's':
							TempData.Set(valueName, newValue);
							break;

						default: continue;
					}

					plainText.Remove(start - removed, last - start + 1 - removed);
					removed += last - start + 1;
				}

			// Mod values
			lines = nestedString.ReadValues2("{mod");
			if (lines.NotEmpty())
				foreach ((string value, int start, int last) in lines)
				{
					if (!parse(value, ':', out string type, out string valueSet)) continue;
					if (!parse(valueSet, '=', out string valueName, out string newValue)) continue;

					switch (type.ToLower()[0])
					{
						case 'i':
							TempData.Set(valueName, TempData.Get(valueName, 0) + int.Parse(newValue));
							break;

						case 'f':
							TempData.Set(valueName, TempData.Get(valueName, 0f) + float.Parse(newValue));
							break;

						case 'd':
							TempData.Set(valueName, TempData.Get(valueName, 0d) + double.Parse(newValue));
							break;

						case 's':
							TempData.Set(valueName, TempData.Get(valueName, 0) + newValue);
							break;

						default: continue;
					}

					plainText.Remove(start - removed, last - start + 1 - removed);
					removed += last - start + 1;
				}

			// Get values
			lines = nestedString.ReadValues2("{get");
			if (lines.NotEmpty())
				foreach ((string value, int start, int last) in lines)
				{
					if (!parse(value, ':', out string type, out string valueName)) continue;
					object obj;

					switch (type.ToLower()[0])
					{
						case 'i':
							obj = TempData.Get(valueName, 0);
							break;

						case 'f':
							obj = TempData.Get(valueName, 0f);
							break;

						case 'd':
							obj = TempData.Get(valueName, 0d);
							break;

						case 'c':
							obj = TempData.Get(valueName, ' ');
							break;

						case 's':
							obj = TempData.Get(valueName, "");
							break;

						default: continue;
					}

					plainText.Remove(start - removed, last - start + 1 - removed);
					string objStr = obj.ToString();

					plainText.Insert(start - removed, objStr);
					removed += last - start + 1 - objStr.Length;
				}

			bool parse(string line, char sep, out string key, out string value)
			{
				string[] args = line.Split(sep, System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
				value = default;
				key = default;

				if (args.IsEmpty() || args.Length < 2) return false;

				value = args[1];
				key = args[0];
				return true;
			}

			return plainText;
		}

		enum Type { String, Float, Int }
		#endregion
		#endregion

		#region Built-In Cmds
		// Return true to skip the signal/delegate emission
		protected virtual bool BuiltInCmd(string cmd, string[] args)
		{
			switch (cmd)
			{
				case "log":
					if (args.NotEmpty()) $"{args[0]}".Log();
					return true;

				default: break;
			}

			return false;
		}
		#endregion

		private enum PrintMode { Direct, Signal, Both, DebugConsole }
	}
}
