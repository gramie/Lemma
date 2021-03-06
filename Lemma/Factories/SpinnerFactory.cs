﻿using System; using ComponentBind;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lemma.Components;
using Microsoft.Xna.Framework;
using BEPUphysics.Paths.PathFollowing;
using Lemma.Util;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Constraints.TwoEntity.Motors;
using BEPUphysics.Constraints.TwoEntity.Joints;
using BEPUphysics.Constraints.SolverGroups;

namespace Lemma.Factories
{
	public class SpinnerFactory : Factory<Main>
	{
		public override Entity Create(Main main)
		{
			Entity entity = new Entity(main, "Spinner");

			entity.Add("MapTransform", new Transform());
			entity.Add("Transform", new Transform());
			entity.Add("Voxel", new DynamicVoxel(0, 0, 0));

			return entity;
		}

		public override void Bind(Entity entity, Main main, bool creating = false)
		{
			Components.Joint joint = entity.GetOrCreate<Components.Joint>("Joint");
			Spinner spinner = entity.GetOrCreate<Spinner>("Spinner");

			spinner.Add(new Binding<Direction>(spinner.Direction, joint.Direction));

			JointFactory.Bind(entity, main, spinner.CreateJoint, true, creating);
			DynamicVoxel voxel = entity.Get<DynamicVoxel>();
			voxel.KineticFriction.Value = voxel.StaticFriction.Value = 0;

			if (main.EditorEnabled)
				spinner.Add(new Binding<Quaternion, Matrix>(spinner.OriginalRotation, x => Quaternion.CreateFromRotationMatrix(x), voxel.Transform));

			entity.Add("Forward", spinner.Forward);
			entity.Add("Backward", spinner.Backward);
			entity.Add("HitMax", spinner.HitMax);
			entity.Add("HitMin", spinner.HitMin);
			entity.Add("MovementLoop", spinner.MovementLoop, new PropertyEntry.EditorData { Options = WwisePicker.Get(main) });
			entity.Add("MovementStop", spinner.MovementStop, new PropertyEntry.EditorData { Options = WwisePicker.Get(main) });

			entity.Add("Direction", joint.Direction);
			entity.Add("Minimum", spinner.Minimum);
			entity.Add("Maximum", spinner.Maximum);
			entity.Add("Locked", spinner.Locked);
			entity.Add("Speed", spinner.Speed);
			entity.Add("Goal", spinner.Goal);
			entity.Add("Servo", spinner.Servo);
			entity.Add("CannotSuspendByDistance", entity.Get<DynamicVoxel>().CannotSuspendByDistance);
		}
	}
}
