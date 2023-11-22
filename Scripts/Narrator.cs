using System.Collections.Generic;
using Godot;

using Cutulu;

namespace Kafka
{
	public partial class Narrator : Node
	{
		[ExportCategory("Essential References")]
		[Export] private Printer Printer;

		[ExportCategory("Custom Elements")]
		[Export] private Control[] VisibleCasts;

		// Identifier of what happens next
		public FollowType FollowType;

		private static Narrator Singleton;

		public KConversation Conversation;
		public KNode KNode;
		public bool Valid;

		[Signal] public delegate void PlayAudioEventHandler(float pitch);
		[Signal] public delegate void SetupEventHandler();
		private bool setup = false;

		private void playVoice(float pitch)
		{
			if (!setup) EmitSignal("Setup");

			EmitSignal("PlayAudio", pitch);
			setup = true;
		}

		public override void _EnterTree()
		{
			Singleton = this;
		}

		#region Load Functions
		public static void SetLocalNode(Node node) { if (node.NotNull() && Singleton.NotNull() && Singleton.Valid) Singleton.KNode.LocalNode = node; }
		public static Node GetLocalNode() => Singleton.NotNull() && Singleton.Valid ? Singleton.KNode.LocalNode : null;
		public static void Load(string globalOrLocalKey, Node source)
		{
			if (Singleton.IsNull()) return;

			Singleton.load(globalOrLocalKey, source);
		}

		/// <summary>
		/// Reads input and parses it to
		/// Loads dialogue from given file and local key
		/// </summary>
		public void load(string pathKey, Node source)
		{
			// Load global
			if (pathKey.Contains(':'))
			{
				string[] args = pathKey.Split(':', System.StringSplitOptions.TrimEntries);
				load(args[0], args[1], source);
			}

			// Load local
			else if (Valid) load(Conversation.LocalPath, pathKey, source);
		}

		/// <summary>
		/// Loads dialogue from given file and local key
		/// </summary>
		public void load(string path, string key, Node source)
		{
			if (path.IsEmpty()) return;
			KNode = null;

			if (Conversation.IsNull() || !path.Trim().Equals(Conversation.LocalPath) || !Conversation.Entries.ContainsKey(key = key.Trim()))
				KConversation.TryLoad(path, key, out Conversation);

			if (Conversation.NotNull())
				Conversation.TryLoad(key, source, out KNode);

			reload();
		}
		#endregion

		#region Narration Functions
		/// <summary>
		/// Reloads user interface based on existing statement
		/// </summary>
		public void reload()
		{
			// Validation
			Valid = Conversation.NotNull() && KNode.NotNull();

			// Follow type
			FollowType = !Valid ? FollowType.Close : KNode._options.NotEmpty() ? FollowType.Options : FollowType.Next;

			// Printer
			if (Printer.NotNull()) Printer.SetActive(Valid);

			if (Valid && Printer.NotNull()) Printer.Print(KNode);

			// For custom visuals
			if (VisibleCasts.NotEmpty())
				foreach (Control c in VisibleCasts)
					if (c.NotNull()) c.Visible = Valid;
		}

		public void decide(int i)
		{
			// Skip if not mean to choose an option
			if (FollowType != FollowType.Options)
				if (FollowType == FollowType.Close) close();
				else next();

			else if (!Valid || KNode.Statement.Options.IsEmpty()) next();
			else
			{
				KeyValuePair<string, string> option = KNode.Statement.Options.GetClampedElement(i);

				// Execute commands
				if (option.Value.NotEmpty() && Printer != null)
					Printer.ExecuteStringCmds(option.Value);

				if (option.Key.IsEmpty()) close();
				else load(option.Key, KNode.LocalNode);
			}
		}

		public void next()
		{
			if (!Valid) return;

			// Skip if no next
			if (FollowType != FollowType.Next)
			{
				close();
				return;
			}

			KeyValuePair<string, string> next = KNode.next;

			// Execute commands
			if (next.Value.NotEmpty() && Printer != null)
				Printer.ExecuteStringCmds(next.Value);

			if (next.Key.IsEmpty()) close();
			else load(next.Key, KNode.LocalNode);
		}

		public void close()
		{
			Printer.SetActive(false);
			Valid = false;
		}
		#endregion

		#region Extra Utility
		public void spell(string str) => spell(str, 1);
		public void spell(string str, float pitch) => spell(str, pitch, .1f);
		public void spell(string str, float pitch, float vocalPitch, float randomRange = .1f)
		{
			// Throw returns
			if (Printer.IsNull()) return;
			if (str.IsEmpty()) return;

			// Char defintion
			char c = str[0];

			// Return cases
			switch (c)
			{
				//case '!': return;
				//case '?': return;
				//case '.': return;
				//case ' ': return;

				default: break;
			}

			// Vocal pitch
			switch (c)
			{
				case 'a': break;
				case 'e': break;
				case 'i': break;
				case 'o': break;
				case 'u': break;

				default:
					vocalPitch = 0;
					break;
			}

			// Setup and Play
			playVoice(pitch + Random.Range(-randomRange, randomRange) + vocalPitch);
		}
		#endregion
	}

	public enum FollowType { Close, Next, Options }
}