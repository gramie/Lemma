﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using BEPUphysics;
using BEPUphysics.Constraints.SolverGroups;
using BEPUphysics.Constraints.TwoEntity.Motors;
using BEPUphysics.UpdateableSystems;
using ComponentBind;
using Lemma.Util;
using Microsoft.Xna.Framework;

namespace Lemma.Components
{
	public class StaticSlider : Component<Main>, IUpdateableComponent
	{
		// Config properties
		public Property<Direction> Direction = new Property<Direction>();
		public Property<int> Minimum = new Property<int>();
		public Property<int> Maximum = new Property<int>();
		public Property<float> Speed = new Property<float>();
		public Property<int> Goal = new Property<int>();
		public Property<uint> MovementLoop = new Property<uint> { Value = AK.EVENTS.SLIDER1_LOOP };
		public Property<uint> MovementStop = new Property<uint> { Value = AK.EVENTS.SLIDER1_STOP };
		public Property<bool> StartAtMinimum = new Property<bool>();
		public Property<Entity.Handle> Parent = new Property<Entity.Handle>();

		// Internal properties
		public Property<Voxel.Coord> Coord = new Property<Voxel.Coord>();
		public Property<float> Position = new Property<float>();

		// I/O properties
		[XmlIgnore]
		public Property<Matrix> Transform = new Property<Matrix>();
		[XmlIgnore]
		public Property<Vector3> LinearVelocity = new Property<Vector3>();

		[XmlIgnore]
		public Property<Matrix> EditorTransform = new Property<Matrix>();

		[XmlIgnore]
		public Command OnHitMin = new Command();

		[XmlIgnore]
		public Command OnHitMax = new Command();

		[XmlIgnore]
		public Command Forward = new Command();

		[XmlIgnore]
		public Command Backward = new Command();

		private float lastPosition;
		private Vector3 lastTranslation;
		private bool playingSound;

		public void Move(int value)
		{
			this.Goal.Value = value;
		}

		public override void Awake()
		{
			base.Awake();
			this.EnabledWhenPaused = false;
			this.EnabledInEditMode = false;

			this.Forward.Action = delegate() { this.Move(this.Maximum); };
			this.Backward.Action = delegate() { this.Move(this.Minimum); };

			if (this.main.EditorEnabled)
			{
				this.Add(new NotifyBinding(delegate()
				{
					Entity parent = this.Parent.Value.Target;
					if (parent == null || parent.Get<Voxel>() == null)
						this.Transform.Value = this.EditorTransform;
					else
						this.Coord.Value = parent.Get<Voxel>().GetCoordinate(this.EditorTransform.Value.Translation);
				}, this.EditorTransform));
			}
			else
			{
				this.Add(new SetBinding<int>(this.Goal, delegate(int value)
				{
					if (!this.playingSound && Math.Abs(this.Position - this.Goal) > 0.5f)
					{
						this.playingSound = true;
						if (this.MovementLoop.Value != 0)
							AkSoundEngine.PostEvent(this.MovementLoop, this.Entity);
					}
				}));
				Action stopMovement = delegate()
				{
					if (this.main.TotalTime > 0.1f && this.MovementStop.Value != 0)
						AkSoundEngine.PostEvent(this.MovementStop, this.Entity);
					else
						AkSoundEngine.PostEvent(AK.EVENTS.STOP_ALL_OBJECT, this.Entity);
					this.playingSound = false;
				};
				this.Add(new CommandBinding(this.OnHitMax, stopMovement));
				this.Add(new CommandBinding(this.OnHitMin, stopMovement));
			}
		}

		public override void Start()
		{
			if (!this.main.EditorEnabled && this.StartAtMinimum)
			{
				this.StartAtMinimum.Value = false;
				this.Position.Value = this.Minimum;
			}

			Binding<Matrix> attachmentBinding = null;
			this.Add(new ChangeBinding<Entity.Handle>(this.Parent, delegate(Entity.Handle old, Entity.Handle value)
			{
				if (attachmentBinding != null)
					this.Remove(attachmentBinding);

				if (value.Target != null)
				{
					Voxel m = value.Target.Get<Voxel>();
					SliderCommon s = m.Entity.Get<SliderCommon>();
					Matrix voxelTransform = s != null ? s.OriginalTransform : m.Transform;

					if (old.Target != null)
					{
						Vector3 pos = this.Transform.Value.Translation;
						Vector3 relativePos = Vector3.Transform(pos, Matrix.Invert(voxelTransform));
						this.Coord.Value = m.GetCoordinateFromRelative(relativePos);
					}

					if (this.main.EditorEnabled)
						this.Coord.Value = m.GetCoordinate(this.EditorTransform.Value.Translation);

					attachmentBinding = new Binding<Matrix>(this.Transform, () => Matrix.CreateTranslation(m.GetRelativePosition(this.Coord) - new Vector3(0.5f) + this.Direction.Value.GetVector() * this.Position) * m.Transform, m.Transform, m.Offset, this.Direction, this.Coord, this.Position);
					this.Add(attachmentBinding);
				}
			}));
		}

		public void Update(float dt)
		{
			float pos = this.Position;
			if (pos < this.Goal)
				pos = Math.Min(pos + this.Speed * dt, this.Goal);
			else if (this.Position > this.Goal)
				pos = Math.Max(pos - this.Speed * dt, this.Goal);
			pos = MathHelper.Clamp(pos, this.Minimum, this.Maximum);

			this.Position.Value = pos;

			Vector3 translation = this.Transform.Value.Translation;
			this.LinearVelocity.Value = (translation - this.lastTranslation) / dt;
			this.lastTranslation = this.Transform.Value.Translation;

			if (this.Position == this.Maximum && this.lastPosition != this.Maximum)
				this.OnHitMax.Execute();
			
			if (this.Position == this.Minimum && this.lastPosition != this.Minimum)
				this.OnHitMin.Execute();

			this.lastPosition = this.Position;
		}
	}
}