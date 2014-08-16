using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using EasyDOC.BLL.Services;
using EasyDOC.Model;
using EasyDOC.Pdf.Interface;
using EasyDOC.Utilities;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Chapter = EasyDOC.Model.Chapter;
using Type = System.Type;

namespace EasyDOC.Pdf.ManualTableGenerators
{
    public class DocumentationDefaultValues
    {
        public static readonly float MarginLeft = 60;
        public static readonly float MarginRight = 70;
        public static readonly float MarginTop = 72;
        public static readonly float MarginBottom = 72;

        public static readonly string Margins = string.Format("{0},{1},{2},{3}", MarginLeft, MarginRight, MarginTop, MarginBottom);

        public static void SetMargins(Document document)
        {
            document.SetMargins(MarginLeft, MarginRight, MarginTop, MarginBottom);
        }
    }

    public class DocumentationPdfCreator
    {
        public Documentation Documentation { get; set; }
        public AbstractChapterService ChapterService { get; set; }
        public string RootDirectory { get; set; }

        private readonly Font[] _headerFonts = 
        {
            PdfFonts.Header1,
            PdfFonts.Header2,
            PdfFonts.Header3
        };

        private readonly Stack<Section> _hierarchy = new Stack<Section>();
        private readonly HtmlToPdf _htmlToPdf;
        private readonly DocxToPdf _docxToPdf;
        private readonly XDocument _config;

        public DocumentationPdfCreator(Documentation documentation, AbstractChapterService chapterService, string rootDirectory)
        {
            Documentation = documentation;
            ChapterService = chapterService;
            RootDirectory = rootDirectory;
            _htmlToPdf = new HtmlToPdf(Documentation, rootDirectory);
            _docxToPdf = new DocxToPdf(Documentation, rootDirectory);

            _config = XDocument.Load(RootDirectory + @"\config\generators.xml");

        }

        private Section _parent;
        private bool _rotate;
        private PdfWriter _pdfWriter;

        private void GenerateChapterHeader(string name, string chapterNumber, string alternateTitle, bool pageBreak, Document document)
        {
            var chapters = chapterNumber.Split('.');
            var level = chapters.Count() - 1;

            var title = string.IsNullOrWhiteSpace(alternateTitle) ? name.ToUpper() : alternateTitle.ToUpper();
            var link = EasyUtilities.SlugifyTitle(title);

            var chunk = new Chunk(title, _headerFonts[level]);
            chunk.SetLocalDestination(link);

            var header = new Paragraph(chunk)
            {
                SpacingBefore = _headerFonts[level].Size * 0.8f,
                SpacingAfter = _headerFonts[level].Size * 0.8f
            };

            if (level == 0)
            {
                if (_parent != null && _hierarchy.Any())
                {
                    document.Add(_hierarchy.Last());

                }
                _hierarchy.Clear();

                var empty = _pdfWriter.PageEmpty;
                var oddNumber = _pdfWriter.CurrentPageNumber % 2 == 1;

                if (!empty && oddNumber)
                {
                    document.Add(Chunk.NEXTPAGE);
                    _pdfWriter.PageEmpty = false;
                }
                else if (empty && !oddNumber)
                {
                    _pdfWriter.PageEmpty = false;
                }

                _parent = new iTextSharp.text.Chapter(header, int.Parse(chapters[0]));
            }
            else
            {
                if (pageBreak)
                {
                    document.Add(Chunk.NEXTPAGE);
                }

                if (level > _hierarchy.Count)
                {
                    _hierarchy.Push(_parent);
                }
                else
                {
                    while (_hierarchy.Count > level)
                    {
                        _parent = _hierarchy.Pop();
                    }
                    _parent = _hierarchy.Peek();
                }

                _parent = _parent.AddSection(header);
                _parent.NumberStyle = Section.NUMBERSTYLE_DOTTED_WITHOUT_FINAL_DOT;
            }
        }

        private void PrintChapter(DocumentationChapter relation, Document document)
        {
            var chapter = relation.Chapter;
            var parameters = GetParameters(relation.Parameters);
            var chapterNumber = relation.ChapterNumber;
            var title = parameters.ContainsKey("title") ? parameters["title"] : chapter.Name;

            if (chapter is Chapter)
            {
                GenerateChapterHeader(title, chapterNumber, relation.Title, relation.NewPage, document);

                if (!string.IsNullOrWhiteSpace((chapter as Chapter).Content))
                {
                    _parent.Add(_htmlToPdf.Parse(ChapterService.GetContent(chapter as Chapter), parameters));
                }

                Flush(document);
            }
            else if (chapter is GeneratedChapter)
            {
                GenerateChapterContent(chapter as GeneratedChapter, chapterNumber, relation.Title, relation.NewPage, parameters, document);
                Flush(document);
            }
            else if (chapter is PdfChapter)
            {
                GenerateChapterHeader(title, chapterNumber, relation.Title, relation.NewPage, document);
                var root = RootDirectory;
                var file = string.Format("{0}/{1}", root, relation.File.GetPath());
                _parent.Add(_docxToPdf.Parse(file));
                Flush(document);
            }

        }

        private void GenerateChapterContent(IEntity chapter, string chapterNumber, string alternateTitle, bool pageBreak, IReadOnlyDictionary<string, string> parameters, Document document)
        {
            var title = string.IsNullOrWhiteSpace(alternateTitle) ? chapter.Name : alternateTitle;

            var settings = _config.Descendants("generator").FirstOrDefault(e => e.Attribute("name").Value == chapter.Name);
            if (settings != null)
            {
                var className = settings.Attribute("class").Value;
                var rotation = false;
                var margins = DocumentationDefaultValues.Margins;

                var pageProperties = settings.Descendants("page").SingleOrDefault();
                if (pageProperties != null)
                {
                    rotation = Boolean.Parse(pageProperties.Attribute("rotation").Value);
                    margins = pageProperties.Attribute("margins").Value;
                }

                var createdInstance = Activator.CreateInstance(Type.GetType(className));

                if (createdInstance is ITableBuilder)
                {
                    var instance = createdInstance as ITableBuilder;
                    instance.Documentation = Documentation;

                    foreach (var arg in settings.Descendants("parameter"))
                    {
                        var name = arg.Attribute("name").Value;
                        var value = arg.Attribute("value").Value;

                        instance.GetType().GetProperty(name).SetValue(instance, value);
                    }

                    instance.GetType().GetProperty("Parameters").SetValue(instance, parameters);

                    var root = instance.GetType().GetProperty("RootDirectory");
                    if (root != null)
                    {
                        root.SetValue(instance, RootDirectory);
                    }

                    SetPageFormat(rotation, margins, document);
                    GenerateChapterHeader(title, chapterNumber, alternateTitle, pageBreak, document);
                    Flush(document);

                    if (instance.HasMultipleTables)
                    {
                        foreach (var table in instance.GetTables())
                        {
                            GenerateChapterHeader(table.Title, chapterNumber + ".1", null, false, document);
                            _parent.Add(table.Table);
                            Flush(document);
                        }
                    }
                    else
                    {
                        document.Add(instance.GetTables().First().Table);
                    }

                    SetPageFormat(false, DocumentationDefaultValues.Margins, document);
                }
            }
            else if (chapter.Name == "Tittel")
            {
                GenerateChapterHeader(title, chapterNumber, alternateTitle, pageBreak, document);
            }
        }

        private void SetPageFormat(bool rotate, string margins, Document document)
        {
            var page = rotate ? PageSize.A4.Rotate() : PageSize.A4;

            var m = margins.Split(',').Select(int.Parse).ToArray();
            document.SetMargins(m[0], m[1], m[2], m[3]);

            if (rotate != _rotate)
            {
                document.SetPageSize(page);
                document.NewPage();
            }

            _rotate = rotate;
        }

        private void Flush(Document document)
        {
            if (_parent != null)
            {
                document.Add(_parent);
                _parent.ElementComplete = true;
                _parent.FlushContent();
            }
        }


        public Stream GetPdf()
        {
            var document = CreateDefaultDocument();
            var tocDocument = CreateDefaultDocument();

            var stream = new MemoryStream();
            _pdfWriter = PdfWriter.GetInstance(document, stream);
            _pdfWriter.SetLinearPageMode();

            var tocStream = new MemoryStream();
            var tocWriter = PdfWriter.GetInstance(tocDocument, tocStream);
            tocWriter.SetLinearPageMode();

            var tocEvent = new PdfEvents(tocDocument, RootDirectory);

            _pdfWriter.PageEvent = tocEvent;
            _pdfWriter.StrictImageSequence = true;

            document.Open();
            tocDocument.Open();

            AddMetaData(document);
            AddChapters(document);
            document.Close();

            var fontFooter = PdfFonts.Small;
            var footer = new Phrase(string.Format("{0}: {1}", Documentation.Project.ProjectNumber, Documentation.Name), fontFooter);
            var pdfReader = new PdfReader(stream.ToArray());

            stream = new MemoryStream();
            var stamper = new PdfStamper(pdfReader, stream);

            var tableOfContents = GenerateTableOfContents(tocEvent, stamper, 0);
            tocDocument.Add(tableOfContents);
            tocDocument.Close();

            var tocReader = new PdfReader(tocStream.ToArray());
            var tocSize = tocReader.NumberOfPages;

            var toc = new ColumnText(null);
            var frontPage = new ColumnText(null);

            var hasFrontPage = Documentation.DocumentationChapters.Any(dc => dc.Chapter is GeneratedChapter && dc.Chapter.Name == "Forside");
            tableOfContents = GenerateTableOfContents(tocEvent, stamper, tocSize + (hasFrontPage ? 2 : 0));
            toc.AddElement(tableOfContents);

            var page = stamper.GetImportedPage(pdfReader, 1);
            int currentPage = 0;
            while (true)
            {
                stamper.InsertPage(++currentPage, pdfReader.GetPageSize(1));
                stamper.GetUnderContent(currentPage).AddTemplate(page, 0, 0);
                toc.Canvas = stamper.GetOverContent(currentPage);
                toc.SetSimpleColumn(60, 72, PageSize.A4.Width - 60, PageSize.A4.Height - 72);
                if (!ColumnText.HasMoreText(toc.Go()))
                {
                    break;
                }
            }

            if (hasFrontPage)
            {
                frontPage.AddElement(new FrontPageBuilder(Documentation, RootDirectory).GetContent());

                page = stamper.GetImportedPage(pdfReader, 1);
                currentPage = 0;
                while (true)
                {
                    stamper.InsertPage(++currentPage, pdfReader.GetPageSize(1));
                    stamper.GetUnderContent(currentPage).AddTemplate(page, 0, 0);
                    frontPage.Canvas = stamper.GetOverContent(currentPage);
                    frontPage.SetSimpleColumn(60, 72, PageSize.A4.Width - 60, PageSize.A4.Height - 72);
                    if (!ColumnText.HasMoreText(frontPage.Go()))
                    {
                        break;
                    }
                }

                stamper.InsertPage(++currentPage, pdfReader.GetPageSize(1));
                stamper.GetUnderContent(currentPage).AddTemplate(page, 0, 0);
            }

            int totalPages = pdfReader.NumberOfPages;

            for (int i = 3; i <= totalPages; ++i)
            {
                var canvas = stamper.GetOverContent(i);
                var table = new PdfPTable(2);

                var rotation = stamper.Reader.GetPageRotation(i);

                table.SetWidths(new[] { 10, 1 });
                table.TotalWidth = (rotation == 0 ? PageSize.A4.Width : PageSize.A4.Height) - 130;
                table.DefaultCell.Border = Rectangle.NO_BORDER;
                table.DefaultCell.FixedHeight = 20;
                table.DefaultCell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(footer);

                table.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                table.AddCell(new Phrase(i.ToString(CultureInfo.InvariantCulture), fontFooter));

                table.WriteSelectedRows(0, -1, 0, -1, 60, 40, canvas);
            }

            stamper.Close();

            var outStream = new MemoryStream(stream.ToArray());
            return outStream;
        }

        private static PdfPTable GenerateTableOfContents(PdfEvents tocEvent, PdfStamper stamper, int tocSize)
        {
            var tableOfContents = new PdfPTable(2);
            tableOfContents.SetWidths(new[] { 10, 1 });
            tableOfContents.WidthPercentage = 100;
            tableOfContents.DefaultCell.Border = Rectangle.NO_BORDER;

            foreach (var item in tocEvent.Toc)
            {
                tableOfContents.DefaultCell.HorizontalAlignment = Element.ALIGN_LEFT;
                tableOfContents.DefaultCell.PaddingLeft = 10 * (item.Level - 1);

                Font font;
                if (item.Level == 0)
                {
                    tableOfContents.DefaultCell.PaddingBottom = 10;
                    tableOfContents.DefaultCell.PaddingTop = 10;
                    font = PdfFonts.Bold;
                }
                else
                {
                    tableOfContents.DefaultCell.PaddingBottom = 2;
                    tableOfContents.DefaultCell.PaddingTop = 2;
                    font = PdfFonts.Normal;
                }

                var chunk = new Chunk(item.Title, font);
                var action = PdfAction.GotoLocalPage(item.Page, new PdfDestination(PdfDestination.FIT), stamper.Writer);
                chunk.SetAction(action);
                tableOfContents.AddCell(new Phrase(chunk));

                tableOfContents.DefaultCell.PaddingLeft = 0;
                tableOfContents.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;

                chunk = new Chunk(string.Format("{0}", item.Page + tocSize), font);
                tableOfContents.AddCell(new Phrase(chunk));
            }
            return tableOfContents;
        }

        private void AddChapters(Document document)
        {
            foreach (var relation in Documentation.DocumentationChapters.OrderBy(rel => rel.ChapterNumber))
            {
                PrintChapter(relation, document);
            }

            if (_parent != null)
                document.Add(_hierarchy.Any() ? _hierarchy.Last() : _parent);
        }

        private void AddMetaData(Document document)
        {
            document.AddCreationDate();
            document.AddAuthor("DYNATEC AS");
            document.AddCreator("EasyDOC 1.0");
            //TODO: set language document.AddLanguage(_doc.Language.Name);
            document.AddTitle(Documentation.Name);
        }

        private static Document CreateDefaultDocument()
        {
            var document = new Document();
            document.SetPageSize(PageSize.A4);
            DocumentationDefaultValues.SetMargins(document);
            return document;
        }

        private static Dictionary<string, string> GetParameters(string parameters)
        {
            var dictionary = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(parameters))
            {
                parameters
                    .Split(';')
                    .Select(p =>
                    {
                        var parts = p.Split('=');
                        return new KeyValuePair<string, string>(parts[0], parts[1].Substring(1, parts[1].Length - 2));
                    })
                    .ToList()
                    .ForEach(kv => dictionary.Add(kv.Key, kv.Value));
            }

            return dictionary;
        }
    }
}