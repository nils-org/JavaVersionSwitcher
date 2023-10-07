using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace JavaVersionSwitcher.Models;

[Serializable]
[DebuggerDisplay("{" + nameof(Version) + "}")]
public class JavaInstallation
{
    public string Location { get; set; }

    [XmlAttribute]
    public string Version { get; set; }
            
    public string FullVersion { get; set; }
}