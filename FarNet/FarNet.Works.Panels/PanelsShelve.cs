﻿/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FarNet.Works
{
	public abstract class ShelveInfo
	{
		//! PSF test.
		public static Collection<ShelveInfo> Stack { get { return new Collection<ShelveInfo>(_Stack); } }
		static readonly List<ShelveInfo> _Stack = new List<ShelveInfo>();

		//! PSF test.
		public string[] GetSelectedNames() { return _SelectedNames; }
		string[] _SelectedNames;

		public abstract string Title { get; }

		public abstract void Pop();

		// current: do provide the current name!
		protected void InitSelected(IAnyPanel panel, string current)
		{
			// get selected
			var files = panel.SelectedList;

			// skip the only selected which is also the current
			if (files.Count == 1 && current != null && current == files[0].Name)
				return;

			// copy names
			_SelectedNames = new string[files.Count];
			for (int i = files.Count; --i >= 0; )
				_SelectedNames[i] = files[i].Name;
		}

	}
}
