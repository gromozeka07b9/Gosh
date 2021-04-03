using System.Collections.Generic;
using System.Xml.Serialization;

namespace QuestHelper.Server.Models.Tracks
{
	[XmlRoot(ElementName="gpx", Namespace="http://www.topografix.com/GPX/1/1")]
	public class GPXCustomScheme {
		[XmlElement(ElementName="metadata", Namespace="http://www.topografix.com/GPX/1/1")]
		public Metadata Metadata { get; set; }
		[XmlElement(ElementName="trk", Namespace="http://www.topografix.com/GPX/1/1")]
		public Trk Trk { get; set; }
		[XmlAttribute(AttributeName="creator")]
		public string Creator { get; set; }
		[XmlAttribute(AttributeName="version")]
		public string Version { get; set; }
		[XmlAttribute(AttributeName="schemaLocation", Namespace="http://www.w3.org/2001/XMLSchema-instance")]
		public string SchemaLocation { get; set; }
		[XmlAttribute(AttributeName="ns3", Namespace="http://www.w3.org/2000/xmlns/")]
		public string Ns3 { get; set; }
		[XmlAttribute(AttributeName="xmlns")]
		public string Xmlns { get; set; }
		[XmlAttribute(AttributeName="xsi", Namespace="http://www.w3.org/2000/xmlns/")]
		public string Xsi { get; set; }
		[XmlAttribute(AttributeName="ns2", Namespace="http://www.w3.org/2000/xmlns/")]
		public string Ns2 { get; set; }
	}
	
	[XmlRoot(ElementName="link", Namespace="http://www.topografix.com/GPX/1/1")]
	public class Link {
		[XmlElement(ElementName="text", Namespace="http://www.topografix.com/GPX/1/1")]
		public string Text { get; set; }
		[XmlAttribute(AttributeName="href")]
		public string Href { get; set; }
	}

	[XmlRoot(ElementName="metadata", Namespace="http://www.topografix.com/GPX/1/1")]
	public class Metadata {
		[XmlElement(ElementName="link", Namespace="http://www.topografix.com/GPX/1/1")]
		public Link Link { get; set; }
		[XmlElement(ElementName="time", Namespace="http://www.topografix.com/GPX/1/1")]
		public string Time { get; set; }
	}

	[XmlRoot(ElementName="TrackPointExtension", Namespace="http://www.garmin.com/xmlschemas/TrackPointExtension/v1")]
	public class TrackPointExtension {
		[XmlElement(ElementName="atemp", Namespace="http://www.garmin.com/xmlschemas/TrackPointExtension/v1")]
		public string Atemp { get; set; }
	}

	[XmlRoot(ElementName="extensions", Namespace="http://www.topografix.com/GPX/1/1")]
	public class Extensions {
		[XmlElement(ElementName="TrackPointExtension", Namespace="http://www.garmin.com/xmlschemas/TrackPointExtension/v1")]
		public TrackPointExtension TrackPointExtension { get; set; }
	}

	[XmlRoot(ElementName="trkpt", Namespace="http://www.topografix.com/GPX/1/1")]
	public class Trkpt {
		[XmlElement(ElementName="ele", Namespace="http://www.topografix.com/GPX/1/1")]
		public string Ele { get; set; }
		[XmlElement(ElementName="time", Namespace="http://www.topografix.com/GPX/1/1")]
		public string Time { get; set; }
		[XmlElement(ElementName="extensions", Namespace="http://www.topografix.com/GPX/1/1")]
		public Extensions Extensions { get; set; }
		[XmlAttribute(AttributeName="lat")]
		public string Lat { get; set; }
		[XmlAttribute(AttributeName="lon")]
		public string Lon { get; set; }
	}

	[XmlRoot(ElementName="trkseg", Namespace="http://www.topografix.com/GPX/1/1")]
	public class Trkseg {
		[XmlElement(ElementName="trkpt", Namespace="http://www.topografix.com/GPX/1/1")]
		public List<Trkpt> Trkpt { get; set; }
	}

	[XmlRoot(ElementName="trk", Namespace="http://www.topografix.com/GPX/1/1")]
	public class Trk {
		[XmlElement(ElementName="name", Namespace="http://www.topografix.com/GPX/1/1")]
		public string Name { get; set; }
		[XmlElement(ElementName="type", Namespace="http://www.topografix.com/GPX/1/1")]
		public string Type { get; set; }
		[XmlElement(ElementName="trkseg", Namespace="http://www.topografix.com/GPX/1/1")]
		public List<Trkseg> Trkseg { get; set; }
	}
}