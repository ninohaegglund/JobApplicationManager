using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;
using JobApplicationManager.API.Features.Applications.DTOs;

namespace JobApplicationManager.API.Features.Exports;

public static class JobApplicationsExcelExporter
{
    private const string SpreadsheetNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public static byte[] CreateWorkbook(IReadOnlyList<JobApplicationResponse> applications)
    {
        using var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteTextEntry(archive, "[Content_Types].xml", ContentTypesXml);
            WriteTextEntry(archive, "_rels/.rels", RootRelationshipsXml);
            WriteTextEntry(archive, "xl/workbook.xml", WorkbookXml);
            WriteTextEntry(archive, "xl/_rels/workbook.xml.rels", WorkbookRelationshipsXml);
            WriteTextEntry(archive, "xl/styles.xml", StylesXml);
            WriteEntry(archive, "xl/worksheets/sheet1.xml", stream => WriteWorksheet(stream, applications));
        }

        return output.ToArray();
    }

    private static void WriteWorksheet(Stream stream, IReadOnlyList<JobApplicationResponse> applications)
    {
        var rowCount = applications.Count + 1;
        var settings = new XmlWriterSettings
        {
            Encoding = Utf8WithoutBom,
            Indent = true
        };

        using var writer = XmlWriter.Create(stream, settings);
        writer.WriteStartDocument(true);
        writer.WriteStartElement("worksheet", SpreadsheetNamespace);

        writer.WriteStartElement("dimension");
        writer.WriteAttributeString("ref", $"A1:G{rowCount}");
        writer.WriteEndElement();

        writer.WriteStartElement("sheetViews");
        writer.WriteStartElement("sheetView");
        writer.WriteAttributeString("workbookViewId", "0");
        writer.WriteStartElement("pane");
        writer.WriteAttributeString("ySplit", "1");
        writer.WriteAttributeString("topLeftCell", "A2");
        writer.WriteAttributeString("activePane", "bottomLeft");
        writer.WriteAttributeString("state", "frozen");
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();

        writer.WriteStartElement("sheetFormatPr");
        writer.WriteAttributeString("defaultRowHeight", "15");
        writer.WriteEndElement();

        WriteColumns(writer);

        writer.WriteStartElement("sheetData");
        WriteHeaderRow(writer);

        for (var i = 0; i < applications.Count; i++)
        {
            WriteApplicationRow(writer, applications[i], i + 2);
        }

        writer.WriteEndElement();

        writer.WriteStartElement("autoFilter");
        writer.WriteAttributeString("ref", $"A1:G{rowCount}");
        writer.WriteEndElement();

        writer.WriteStartElement("pageMargins");
        writer.WriteAttributeString("left", "0.7");
        writer.WriteAttributeString("right", "0.7");
        writer.WriteAttributeString("top", "0.75");
        writer.WriteAttributeString("bottom", "0.75");
        writer.WriteAttributeString("header", "0.3");
        writer.WriteAttributeString("footer", "0.3");
        writer.WriteEndElement();

        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    private static void WriteColumns(XmlWriter writer)
    {
        var widths = new[] { 10, 28, 30, 18, 60, 24, 24 };

        writer.WriteStartElement("cols");
        for (var i = 0; i < widths.Length; i++)
        {
            writer.WriteStartElement("col");
            writer.WriteAttributeString("min", (i + 1).ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("max", (i + 1).ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("width", widths[i].ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("customWidth", "1");
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    private static void WriteHeaderRow(XmlWriter writer)
    {
        writer.WriteStartElement("row");
        writer.WriteAttributeString("r", "1");
        writer.WriteAttributeString("spans", "1:7");

        WriteInlineStringCell(writer, 1, 1, "Id", isHeader: true);
        WriteInlineStringCell(writer, 1, 2, "Company", isHeader: true);
        WriteInlineStringCell(writer, 1, 3, "Role", isHeader: true);
        WriteInlineStringCell(writer, 1, 4, "Status", isHeader: true);
        WriteInlineStringCell(writer, 1, 5, "Notes", isHeader: true);
        WriteInlineStringCell(writer, 1, 6, "Created At (UTC)", isHeader: true);
        WriteInlineStringCell(writer, 1, 7, "Updated At (UTC)", isHeader: true);

        writer.WriteEndElement();
    }

    private static void WriteApplicationRow(XmlWriter writer, JobApplicationResponse application, int rowIndex)
    {
        writer.WriteStartElement("row");
        writer.WriteAttributeString("r", rowIndex.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("spans", "1:7");

        WriteNumberCell(writer, rowIndex, 1, application.Id);
        WriteInlineStringCell(writer, rowIndex, 2, application.CompanyName);
        WriteInlineStringCell(writer, rowIndex, 3, application.RoleTitle);
        WriteInlineStringCell(writer, rowIndex, 4, application.Status);
        WriteInlineStringCell(writer, rowIndex, 5, application.Notes ?? string.Empty);
        WriteInlineStringCell(writer, rowIndex, 6, FormatDateTime(application.CreatedAt));
        WriteInlineStringCell(writer, rowIndex, 7, application.UpdatedAt is null ? string.Empty : FormatDateTime(application.UpdatedAt.Value));

        writer.WriteEndElement();
    }

    private static void WriteNumberCell(XmlWriter writer, int rowIndex, int columnIndex, int value)
    {
        writer.WriteStartElement("c");
        writer.WriteAttributeString("r", GetCellReference(columnIndex, rowIndex));
        writer.WriteStartElement("v");
        writer.WriteString(value.ToString(CultureInfo.InvariantCulture));
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteInlineStringCell(XmlWriter writer, int rowIndex, int columnIndex, string value, bool isHeader = false)
    {
        writer.WriteStartElement("c");
        writer.WriteAttributeString("r", GetCellReference(columnIndex, rowIndex));
        if (isHeader)
        {
            writer.WriteAttributeString("s", "1");
        }

        writer.WriteAttributeString("t", "inlineStr");
        writer.WriteStartElement("is");
        writer.WriteStartElement("t");
        if (PreservesWhitespace(value))
        {
            writer.WriteAttributeString("xml", "space", XmlNamespace, "preserve");
        }

        writer.WriteString(value);
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static string FormatDateTime(DateTime value)
    {
        var dateTime = value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : value;
        return dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    }

    private static bool PreservesWhitespace(string value)
    {
        return value.Length > 0 && (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1]));
    }

    private static string GetCellReference(int columnIndex, int rowIndex)
    {
        var columnName = new StringBuilder();
        while (columnIndex > 0)
        {
            columnIndex--;
            columnName.Insert(0, (char)('A' + columnIndex % 26));
            columnIndex /= 26;
        }

        return $"{columnName}{rowIndex}";
    }

    private static void WriteEntry(ZipArchive archive, string entryName, Action<Stream> write)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
        using var stream = entry.Open();
        write(stream);
    }

    private static void WriteTextEntry(ZipArchive archive, string entryName, string content)
    {
        WriteEntry(archive, entryName, stream =>
        {
            using var writer = new StreamWriter(stream, Utf8WithoutBom);
            writer.Write(content);
        });
    }

    private const string ContentTypesXml =
        """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
          <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
          <Default Extension="xml" ContentType="application/xml"/>
          <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
          <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
          <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
        </Types>
        """;

    private const string RootRelationshipsXml =
        """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
          <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
        </Relationships>
        """;

    private const string WorkbookXml =
        """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
          <sheets>
            <sheet name="Applications" sheetId="1" r:id="rId1"/>
          </sheets>
        </workbook>
        """;

    private const string WorkbookRelationshipsXml =
        """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
          <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
          <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
        </Relationships>
        """;

    private const string StylesXml =
        """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
          <fonts count="2">
            <font>
              <sz val="11"/>
              <color theme="1"/>
              <name val="Calibri"/>
              <family val="2"/>
            </font>
            <font>
              <b/>
              <sz val="11"/>
              <color rgb="FFFFFFFF"/>
              <name val="Calibri"/>
              <family val="2"/>
            </font>
          </fonts>
          <fills count="3">
            <fill>
              <patternFill patternType="none"/>
            </fill>
            <fill>
              <patternFill patternType="gray125"/>
            </fill>
            <fill>
              <patternFill patternType="solid">
                <fgColor rgb="FF1F4E78"/>
                <bgColor indexed="64"/>
              </patternFill>
            </fill>
          </fills>
          <borders count="1">
            <border>
              <left/>
              <right/>
              <top/>
              <bottom/>
              <diagonal/>
            </border>
          </borders>
          <cellStyleXfs count="1">
            <xf numFmtId="0" fontId="0" fillId="0" borderId="0"/>
          </cellStyleXfs>
          <cellXfs count="2">
            <xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/>
            <xf numFmtId="0" fontId="1" fillId="2" borderId="0" xfId="0" applyFont="1" applyFill="1"/>
          </cellXfs>
          <cellStyles count="1">
            <cellStyle name="Normal" xfId="0" builtinId="0"/>
          </cellStyles>
        </styleSheet>
        """;
}
