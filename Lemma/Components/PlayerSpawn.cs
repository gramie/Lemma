﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Serialization;
using ComponentBind;

namespace Lemma.Components
{
	public class PlayerSpawn : Component<Main>
	{
		public Property<bool> IsActivated = new Property<bool>();
		public Property<Vector3> Position = new Property<Vector3>();
		public Property<float> Rotation = new Property<float>();

		[XmlIgnore]
		public Command Activate = new Command();
		[XmlIgnore]
		public Command Deactivate = new Command();
		[XmlIgnore]
		public Command OnSpawn = new Command();

		private static List<PlayerSpawn> spawns = new List<PlayerSpawn>();

		public static PlayerSpawn FirstActive()
		{
			return PlayerSpawn.spawns.FirstOrDefault(x => x.IsActivated);
		}

		public void EditorProperties()
		{
			this.Entity.Add("IsActivated", this.IsActivated);
			this.Entity.Add("Activate", this.Activate);
			this.Entity.Add("Deactivate", this.Deactivate);
			this.Entity.Add("OnSpawn", this.OnSpawn);
		}

		public override void Awake()
		{
			base.Awake();
			this.EnabledInEditMode = false;
			this.EnabledWhenPaused = false;
			PlayerSpawn.spawns.Add(this);

			this.Add(new CommandBinding(this.Delete, delegate() { PlayerSpawn.spawns.Remove(this); }));

			this.Activate.Action = delegate()
			{
				this.IsActivated.Value = true;
			};

			this.Deactivate.Action = delegate()
			{
				this.IsActivated.Value = false;
			};

			this.Add(new SetBinding<bool>(this.IsActivated, delegate(bool value)
			{
				if (value)
				{
					foreach (PlayerSpawn spawn in PlayerSpawn.spawns)
					{
						if (spawn != this)
							spawn.Deactivate.Execute();
					}
				}
			}));
		}
	}
}
