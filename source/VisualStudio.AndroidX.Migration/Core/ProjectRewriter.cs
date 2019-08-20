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
        private IProgress<string> progress;

		public ProjectRewriter(ITranslationResolver resolver, IProgress<string> progress)
		{
			this.resolver = resolver;
            this.progress = progress;
		}

		public void RewriteProject(string projectFileName, bool addMigrationNuget)
		{ 
			var xml = RewriteCSProj(File.ReadAllText(projectFileName), addMigrationNuget);
			File.WriteAllText(projectFileName, xml);
		}

		public string RewriteCSProj(string csproj, bool addMigrationNuget = false)
		{
            bool hasAndroidNugets = false;
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
                        hasAndroidNugets = true;
                        progress?.Report($"Migrating Nuget {androidNuget}");
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
                if (addMigrationNuget && hasAndroidNugets)
                {
                    addMigrationNuget = false;
                    var node = doc.CreateNode(XmlNodeType.Element, "PackageReference", references.First().NamespaceURI);
                    var includeAttribute = doc.CreateAttribute("Include");
                    includeAttribute.Value = "Xamarin.AndroidX.Migration";
                    node.Attributes.Append(includeAttribute);
                    var versionAttribute = doc.CreateAttribute("Version");
                    versionAttribute.Value = "1.0.0-preview03";
                    node.Attributes.Append(versionAttribute);
                    group.AppendChild(node);
                }
			}
			return doc.InnerXml;
		}
	}
}
