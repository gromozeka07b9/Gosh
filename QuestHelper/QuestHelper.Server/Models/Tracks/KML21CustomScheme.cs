using System.Collections.Generic;
using System.Xml.Serialization;

namespace QuestHelper.Server.Models.Tracks
{
    [XmlRoot(ElementName="kml", Namespace="http://earth.google.com/kml/2.1")]
    public class Kml21CustomScheme {
        [XmlElement(ElementName="Folder", Namespace="http://earth.google.com/kml/2.1")]
        public Folder Folder { get; set; }
        [XmlAttribute(AttributeName="schemaLocation", Namespace="http://www.w3.org/2001/XMLSchema-instance")]
        public string SchemaLocation { get; set; }
        [XmlAttribute(AttributeName="xsi", Namespace="http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
        [XmlAttribute(AttributeName="xmlns")]
        public string Xmlns { get; set; }
    }
    
	[XmlRoot(ElementName="LineStyle", Namespace="http://earth.google.com/kml/2.1")]
	public class LineStyle {
		[XmlElement(ElementName="color", Namespace="http://earth.google.com/kml/2.1")]
		public string Color { get; set; }
		[XmlElement(ElementName="width", Namespace="http://earth.google.com/kml/2.1")]
		public string Width { get; set; }
	}

	[XmlRoot(ElementName="Style", Namespace="http://earth.google.com/kml/2.1")]
	public class Style {
		[XmlElement(ElementName="LineStyle", Namespace="http://earth.google.com/kml/2.1")]
		public LineStyle LineStyle { get; set; }
		[XmlElement(ElementName="IconStyle", Namespace="http://earth.google.com/kml/2.1")]
		public IconStyle IconStyle { get; set; }
		[XmlElement(ElementName="ListStyle", Namespace="http://earth.google.com/kml/2.1")]
		public ListStyle ListStyle { get; set; }
	}

	[XmlRoot(ElementName="LineString", Namespace="http://earth.google.com/kml/2.1")]
	public class LineString {
		[XmlElement(ElementName="extrude", Namespace="http://earth.google.com/kml/2.1")]
		public string Extrude { get; set; }
		[XmlElement(ElementName="tessellate", Namespace="http://earth.google.com/kml/2.1")]
		public string Tessellate { get; set; }
		[XmlElement(ElementName="altitudeMode", Namespace="http://earth.google.com/kml/2.1")]
		public string AltitudeMode { get; set; }
		[XmlElement(ElementName="coordinates", Namespace="http://earth.google.com/kml/2.1")]
		public string Coordinates { get; set; }
	}

	[XmlRoot(ElementName="Placemark", Namespace="http://earth.google.com/kml/2.1")]
	public class Placemark {
		[XmlElement(ElementName="name", Namespace="http://earth.google.com/kml/2.1")]
		public string Name { get; set; }
		[XmlElement(ElementName="Style", Namespace="http://earth.google.com/kml/2.1")]
		public Style Style { get; set; }
		[XmlElement(ElementName="LineString", Namespace="http://earth.google.com/kml/2.1")]
		public LineString LineString { get; set; }
		[XmlElement(ElementName="description", Namespace="http://earth.google.com/kml/2.1")]
		public string Description { get; set; }
		[XmlElement(ElementName="Point", Namespace="http://earth.google.com/kml/2.1")]
		public Point Point { get; set; }
		[XmlElement(ElementName="TimeSpan", Namespace="http://earth.google.com/kml/2.1")]
		public TimeSpanKml TimeSpanKml { get; set; }
	}

	[XmlRoot(ElementName="Icon", Namespace="http://earth.google.com/kml/2.1")]
	public class Icon {
		[XmlElement(ElementName="href", Namespace="http://earth.google.com/kml/2.1")]
		public string Href { get; set; }
	}

	[XmlRoot(ElementName="hotSpot", Namespace="http://earth.google.com/kml/2.1")]
	public class HotSpot {
		[XmlAttribute(AttributeName="x")]
		public string X { get; set; }
		[XmlAttribute(AttributeName="xunits")]
		public string Xunits { get; set; }
		[XmlAttribute(AttributeName="y")]
		public string Y { get; set; }
		[XmlAttribute(AttributeName="yunits")]
		public string Yunits { get; set; }
	}

	[XmlRoot(ElementName="IconStyle", Namespace="http://earth.google.com/kml/2.1")]
	public class IconStyle {
		[XmlElement(ElementName="scale", Namespace="http://earth.google.com/kml/2.1")]
		public string Scale { get; set; }
		[XmlElement(ElementName="Icon", Namespace="http://earth.google.com/kml/2.1")]
		public Icon Icon { get; set; }
		[XmlElement(ElementName="hotSpot", Namespace="http://earth.google.com/kml/2.1")]
		public HotSpot HotSpot { get; set; }
		[XmlElement(ElementName="color", Namespace="http://earth.google.com/kml/2.1")]
		public string Color { get; set; }
	}

	[XmlRoot(ElementName="Point", Namespace="http://earth.google.com/kml/2.1")]
	public class Point {
		[XmlElement(ElementName="coordinates", Namespace="http://earth.google.com/kml/2.1")]
		public string Coordinates { get; set; }
		[XmlElement(ElementName="altitudeMode", Namespace="http://earth.google.com/kml/2.1")]
		public string AltitudeMode { get; set; }
	}

	[XmlRoot(ElementName="Folder", Namespace="http://earth.google.com/kml/2.1")]
	public class Folder {
		[XmlElement(ElementName="name", Namespace="http://earth.google.com/kml/2.1")]
		public string Name { get; set; }
		[XmlElement(ElementName="description", Namespace="http://earth.google.com/kml/2.1")]
		public string Description { get; set; }
		[XmlElement(ElementName="Placemark", Namespace="http://earth.google.com/kml/2.1")]
		public List<Placemark> Placemark { get; set; }
		[XmlElement(ElementName="Style", Namespace="http://earth.google.com/kml/2.1")]
		public Style Style { get; set; }
		[XmlElement(ElementName="Folder", Namespace="http://earth.google.com/kml/2.1")]
		public List<FolderInside> FolderInside { get; set; }
	}

	[XmlRoot(ElementName="Folder", Namespace="http://earth.google.com/kml/2.1")]
	public class FolderInside {
		[XmlElement(ElementName="name", Namespace="http://earth.google.com/kml/2.1")]
		public string Name { get; set; }
		[XmlElement(ElementName="description", Namespace="http://earth.google.com/kml/2.1")]
		public string Description { get; set; }
		[XmlElement(ElementName="Placemark", Namespace="http://earth.google.com/kml/2.1")]
		public List<Placemark> Placemark { get; set; }
		[XmlElement(ElementName="Style", Namespace="http://earth.google.com/kml/2.1")]
		public Style Style { get; set; }
	}
	
	[XmlRoot(ElementName="ListStyle", Namespace="http://earth.google.com/kml/2.1")]
	public class ListStyle {
		[XmlElement(ElementName="listItemType", Namespace="http://earth.google.com/kml/2.1")]
		public string ListItemType { get; set; }
	}

	[XmlRoot(ElementName="TimeSpan", Namespace="http://earth.google.com/kml/2.1")]
	public class TimeSpanKml
	{
		[XmlElement(ElementName="begin", Namespace="http://earth.google.com/kml/2.1")]
		public string Begin { get; set; }
		[XmlElement(ElementName="end", Namespace="http://earth.google.com/kml/2.1")]
		public string End { get; set; }
	}
}