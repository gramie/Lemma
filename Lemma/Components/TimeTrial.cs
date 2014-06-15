﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using ComponentBind;
using Lemma.GInterfaces;

namespace Lemma.Components
{
	public class TimeTrial : Component<Main>
	{
		private TimeTrialUI theUI = null;

		[XmlIgnore]
		public Command StartTimeTrial = new Command();

		[XmlIgnore]
		public Command EndTimeTrial = new Command();

		[XmlIgnore]
		public Command PauseTimer = new Command();

		[XmlIgnore]
		public Command ResumeTimer = new Command();

		public override void Awake()
		{
			this.theUI = new TimeTrialUI();
			main.AddComponent(theUI);
			this.Add(new CommandBinding(StartTimeTrial, () =>
			{
				theUI.ElapsedTime.Value = 0f;
				theUI.AnimateIn();
			}));
			this.Add(new CommandBinding(EndTimeTrial, () =>
			{
				theUI.AnimateOut(true);
			}));
			this.Add(new CommandBinding(PauseTimer, () =>
			{
				theUI.TimeTrialTicking.Value = false;
			}));

			this.Add(new CommandBinding(ResumeTimer, () =>
			{
				theUI.TimeTrialTicking.Value = true;
			}));
			base.Awake();
		}

		public override void delete()
		{
			main.RemoveComponent(theUI);
			base.delete();
		}
	}
}