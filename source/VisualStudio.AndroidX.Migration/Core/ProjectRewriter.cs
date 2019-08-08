using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml;
using System.IO;

namespace VisualStudio.AndroidX.Migration
{
	public class ProjectRewriter
	{
		private ITranslationResolver resolver;

		public ProjectRewriter(ITranslationResolver resolver)
		{
			this.resolver = resolver;
		}

		public void RewriteProject(string projectFileName)
		{ 
			var xml = RewriteCSProj(File.ReadAllText(projectFileName));
			File.WriteAllText(projectFileName, xml);
		}

		public string RewriteCSProj(string csproj)
		{
			var doc = new XmlDocument();
			doc.PreserveWhitespace = true;
			doc.LoadXml(csproj);
			var itemGroups = doc.GetElementsByTagName("ItemGroup");
			foreach (var group in itemGroups.OfType<XmlElement>())
			{
				var references = group.ChildNodes.OfType<XmlElement>().Where(c => c.Name == "PackageReference");
				foreach (var reference in references)
				{
					var androidNuget = reference.Attributes["Include"].InnerText;
					if (resolver.Nugets.ContainsKey(androidNuget))
					{
						reference.Attributes["Include"].InnerText = resolver.Nugets[reference.Attributes["Include"].InnerText].Key;
						var androidXVersion = resolver.Nugets[androidNuget].Value;
						if (reference.HasAttribute("Version"))
						{
							reference.Attributes["Version"].InnerText = androidXVersion;
						} 
						else 
						{
							var version = reference.ChildNodes.OfType<XmlElement>().FirstOrDefault(n => n.Name == "Version");
							if (version != null)
							{ 
								version.InnerText = androidXVersion;
							}
							else
							{
								var versionAttribute = doc.CreateAttribute("Version");
								versionAttribute.InnerText = androidXVersion;
								reference.Attributes.Append(versionAttribute);
							}
						}
					}
				}
			}
			return doc.InnerXml;
		}
	}
}
