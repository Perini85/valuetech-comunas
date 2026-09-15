using System.Xml;
using System.Xml.Linq;
using Valuetech.Domain.Entities;

namespace Valuetech.Infrastructure.Persistence.SqlServer;

internal static class InformacionAdicionalXml
{
    public static string? Serialize(InformacionAdicional? info) => info is null ? null :
        new XElement("Info",
            new XElement("Superficie", info.Superficie),
            new XElement("Poblacion", new XAttribute("Densidad", info.Densidad), info.Poblacion))
        .ToString(SaveOptions.DisableFormatting);

    public static InformacionAdicional? Deserialize(string? xml)
    {
        if (string.IsNullOrWhiteSpace(xml)) return null;
        using var reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        });
        var root = XElement.Load(reader);
        return new InformacionAdicional(
            (decimal?)root.Element("Superficie") ?? throw new FormatException("XML sin superficie."),
            (int?)root.Element("Poblacion") ?? throw new FormatException("XML sin población."),
            (decimal?)root.Element("Poblacion")?.Attribute("Densidad") ?? throw new FormatException("XML sin densidad."));
    }
}
