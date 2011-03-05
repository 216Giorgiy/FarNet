﻿
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar
{
	sealed class DataExplorer : TableExplorer
	{
		const string TypeIdString = "f61f95e9-42fc-4285-9858-3ec7c0d7a58e";
		internal DataPanel Panel { get; set; }
		public DataExplorer()
			: base(new Guid(TypeIdString))
		{
			FileComparer = new FileDataComparer();
			Location = "*";
			Functions =
				ExplorerFunctions.DeleteFiles |
				ExplorerFunctions.CreateFile |
				ExplorerFunctions.ExportFile;
		}
		///
		public override Panel DoCreatePanel()
		{
			throw new ModuleException("Data panel is not yet supported.");
		}
		///
		public override IList<FarFile> DoGetFiles(ExplorerEventArgs args)
		{
			return Panel.Explore(args);
		}
		///
		public override void DoDeleteFiles(DeleteFilesEventArgs args)
		{
			Panel.DoDeleteFiles(args);
		}
		///
		public override void DoCreateFile(CreateFileEventArgs args)
		{
			args.Result = JobResult.Ignore;
			Panel.DoCreateFile();
		}
		///
		public override void DoExportFile(ExportFileEventArgs args) //????? move to base
		{
			if (args == null) return;
			A.WriteFormatList(args.File.Data, args.FileName);
		}
	}
}