﻿
/* Copyright 2012-2014 Roman Kuzmin
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections;
using System.Data.Common;
using System.IO;
using System.Text;
using MarkdownDeep;

namespace MarkdownToHtml
{
	class Program
	{
		static int Main(string[] args)
		{
			var parameters = string.Join("; ", args);
			
			string from = null;
			string to = null;
			string title = null;
			try
			{
				var builder = new DbConnectionStringBuilder() { ConnectionString = parameters };
				foreach (DictionaryEntry it in (IDictionary)builder)
				{
					switch (it.Key.ToString())
					{
						case "from": from = it.Value.ToString(); break;
						case "to": to = it.Value.ToString(); break;
						case "title": title = it.Value.ToString(); break;
						default: throw new ArgumentException("Unknown key: " + it.Key);
					}
				}

				if (from == null) throw new ArgumentException("Missing argument: From=<Markdown file>");
				if (to == null) throw new ArgumentException("Missing argument: To=<HTML file>");
				if (title == null) title = Path.GetFileNameWithoutExtension(from);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(string.Format(null, "Invalid command line. Parameter string: '{0}'. Error: {1}", parameters, e.Message));
				return 1;
			}

			try
			{
				var text = File.ReadAllText(from);

				var markdown = new Markdown();
				markdown.ExtraMode = true;

				var html = markdown.Transform(text);

				using (var writer = new StreamWriter(to, false, Encoding.UTF8))
				{
					writer.WriteLine("<html><title>{0}</title><body>", title);
					writer.Write(html);
					writer.WriteLine("</body></html>");
				}

				return 0;
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e.Message);
				return 1;
			}
		}
	}
}
