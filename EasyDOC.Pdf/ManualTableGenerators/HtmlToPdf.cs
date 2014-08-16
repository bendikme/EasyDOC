using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EasyDOC.Model;
using EasyDOC.Utilities;
using HtmlAgilityPack;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Font = iTextSharp.text.Font;
using Image = iTextSharp.text.Image;

namespace EasyDOC.Pdf.ManualTableGenerators
{
    class HtmlToPdf
    {
        private readonly Documentation _doc;
        private readonly string _root;
        private Stack<ITextElementArray> _contexts;
        private Stack<Dictionary<string, string>> _styles;
        private Stack<TableState> _tables;
        private Dictionary<string, string> _parameters;

        private bool _ignoreContent;

        public HtmlToPdf(Documentation doc, string root)
        {
            _doc = doc;
            _root = root;
        }

        public IElement Parse(string content, Dictionary<string, string> parameters)
        {
            _parameters = parameters;

            var document = new HtmlDocument();

            content = content.Replace("\n", "");
            content = content.Replace("\r", "");
            content = content.Replace("\t", "");
            content = content.Replace("&nbsp;", " ");

            document.LoadHtml(content);

            _contexts = new Stack<ITextElementArray>();
            _contexts.Push(new Paragraph());

            _tables = new Stack<TableState>();
            _styles = new Stack<Dictionary<string, string>>();

            ParseNode(document.DocumentNode, new DocumentState(new FontStyle()));
            return _contexts.Last();
        }

        private void ParseNode(HtmlNode node, DocumentState state)
        {

            switch (node.NodeType)
            {
                case HtmlNodeType.Document:
                case HtmlNodeType.Element:

                    _ignoreContent = false;
                    var styles = ParseStyles(GetAttributeValue(node, "style"));

                    _styles.Push(styles);

                    switch (node.Name)
                    {
                        case "a":
                            var url = GetAttributeValue(node, "href");
                            var anchor = new Anchor
                            {
                                Reference = url
                            };
                            _contexts.Push(anchor);
                            break;

                        case "br":
                            _contexts.Peek().Add(Chunk.NEWLINE);
                            break;

                        case "p":
                            var paragraph = new Paragraph
                            {
                                SpacingAfter = 8,
                                SpacingBefore = 8
                            };
                            ApplyStyles(paragraph, styles);
                            _contexts.Push(paragraph);
                            break;

                        case "img":
                            var src = GetAttributeValue(node, "src");

                            if (!EasyUtilities.IsAbsoluteUrl(src))
                            {
                                src = _root + "/" + src;
                            }

                            try
                            {
                                var image = Image.GetInstance(src);
                                ApplyStyles(image, styles);
                                _contexts.Peek().Add(image);
                            }
                            catch
                            {
                                // TODO: Add error list
                            }

                            break;

                        case "strong": state.Font.Bold = true; break;
                        case "em": state.Font.Italic = true; break;
                        case "u": state.Font.Underlined = true; break;

                        case "ul":
                            ++state.ListLevel;
                            var list = new List
                            {
                                IndentationLeft = 10 * state.ListLevel,
                                ListSymbol = new Chunk("•  ")
                            };
                            _contexts.Push(list);
                            _ignoreContent = true;
                            break;

                        case "li":
                            var item = new ListItem
                            {
                                SpacingAfter = 4,
                                SpacingBefore = 4,
                                IndentationRight = 20
                            };
                            ApplyStyles(item, styles);
                            _contexts.Push(item);
                            break;

                        case "table":
                            var table = new TableState
                            {
                                BorderWidth = GetAttributeInteger(node, "border"),
                                CellSpacing = GetAttributeInteger(node, "cellspacing"),
                                CellPadding = GetAttributeInteger(node, "cellpadding")
                            };

                            _tables.Push(table);
                            _ignoreContent = true;
                            break;

                        case "tr":
                            _ignoreContent = true;
                            break;

                        case "th":
                        case "td":
                            _contexts.Push(new PdfElementStore());
                            break;
                    }

                    foreach (var child in node.ChildNodes)
                        ParseNode(child, state);

                    switch (node.Name)
                    {
                        case "a":
                        case "p":
                        case "li":
                            var element = _contexts.Pop();
                            _contexts.Peek().Add(element);
                            break;

                        case "ul":
                            --state.ListLevel;
                            element = _contexts.Pop();
                            _contexts.Peek().Add(element);
                            break;

                        case "strong": state.Font.Bold = false; break;
                        case "em": state.Font.Italic = false; break;
                        case "u": state.Font.Underlined = false; break;

                        case "table":
                            var info = _tables.Pop();
                            var table = new PdfPTable(info.ColumnCount)
                            {
                                SpacingAfter = 8,
                                SpacingBefore = 8,
                                WidthPercentage = 100
                            };

                            table.SetWidths(info.ColumnWidths.ToArray());

                            foreach (var cell in info.Cells)
                            {
                                table.AddCell(cell);
                            }

                            _contexts.Peek().Add(table);
                            break;

                        case "tr":
                            _tables.Peek().ColumnsCounted = true;
                            _ignoreContent = true;
                            break;

                        case "td":
                        case "th":

                            int colspan;
                            int rowspan;

                            var currentTable = _tables.Peek();

                            if (!int.TryParse(GetAttributeValue(node, "colspan"), out colspan)) colspan = 1;
                            if (!int.TryParse(GetAttributeValue(node, "rowspan"), out rowspan)) rowspan = 1;

                            if (!currentTable.ColumnsCounted)
                            {
                                currentTable.ColumnCount += colspan;

                                if (styles.ContainsKey("width"))
                                {
                                    var value = styles["width"];
                                    value = value.Replace("px", "");
                                    value = value.Replace("%", "");
                                    value = value.Replace("pt","");
                                    currentTable.ColumnWidths.Add((int)Math.Round(float.Parse(value)));
                                }
                            }

                            var item = _contexts.Pop() as PdfElementStore;
                            PdfPCell newCell;

                            if (item.Elements.Count == 1 && item.Elements[0] is Image)
                            {
                                var e = item.Elements[0];
                                newCell = new PdfPCell(e as Image, false)
                                {
                                    Colspan = colspan,
                                    Rowspan = rowspan,
                                    BorderWidth = currentTable.BorderWidth,
                                    Padding = currentTable.CellPadding,
                                    Indent = currentTable.CellSpacing,
                                    VerticalAlignment = Element.ALIGN_MIDDLE
                                };
                            }
                            else
                            {
                                var paragraph = new Paragraph();
                                newCell = new PdfPCell(paragraph)
                                {
                                    Colspan = colspan,
                                    Rowspan = rowspan,
                                    BorderWidth = currentTable.BorderWidth,
                                    Padding = currentTable.CellPadding,
                                    Indent = currentTable.CellSpacing,
                                    VerticalAlignment = Element.ALIGN_MIDDLE
                                };

                                ApplyStyles(paragraph, styles);
                                foreach (var e in item.Elements)
                                {
                                    paragraph.Add(e);
                                }

                                newCell.AddElement(paragraph);
                            }

                            ApplyStyles(newCell, styles);
                            currentTable.Cells.Add(newCell);
                            _ignoreContent = true;
                            break;
                    }

                    _styles.Pop();
                    break;

                case HtmlNodeType.Text:
                    if (!_ignoreContent)
                    {
                        var text = HtmlEntity.DeEntitize(node.InnerText);
                        text = ReplaceVariables(text, _parameters);

                        if (text.Contains("££"))
                        {
                            var firstPart = text.IndexOf("££", StringComparison.Ordinal);
                            if (firstPart > 0)
                            {
                                var firstPartText = text.Substring(0, firstPart);
                                _contexts.Peek().Add(CreateTextChunk(state, firstPartText));
                            }

                            text = text.Substring(firstPart + 2);
                            _contexts.Peek().Add(Chunk.NEXTPAGE);
                        }

                        _contexts.Peek().Add(CreateTextChunk(state, text));
                    }
                    break;
            }
        }

        private Chunk CreateTextChunk(DocumentState state, string text)
        {
            var chunk = new Chunk(text)
            {
                Font = GetFont(state.Font)
            };

            if (_styles.Count > 0)
            {
                var colorStyle = _styles.FirstOrDefault(kv => kv.ContainsKey("color"));
                if (colorStyle != null)
                    chunk.Font.Color = new BaseColor(ParseColor(colorStyle["color"]));
            }
            return chunk;
        }

        private static int GetAttributeInteger(HtmlNode node, string attribute)
        {
            var value = GetAttributeValue(node, attribute);
            int integer;
            return int.TryParse(value, out integer) ? integer : 0;
        }

        private string ReplaceVariables(string text, Dictionary<string, string> parameters)
        {
            text = text.Replace("$ProjectName", _doc.Project.Name);
            text = text.Replace("$ProjectNumber", _doc.Project.ProjectNumber);
            text = text.Replace("$ProjectCustomer", _doc.Project.Customer.Name);

            return parameters.Aggregate(text, (current, pair) => current.Replace("$" + pair.Key, pair.Value));
        }

        private static Color ParseColor(string color)
        {
            if (color.StartsWith("#"))
            {
                return (Color)new ColorConverter().ConvertFromString(color);
            }

            if (color.StartsWith("rgb"))
            {
                var start = color.IndexOf('(') + 1;
                var values = color
                    .Substring(start, color.IndexOf(')') - start)
                    .Split(',')
                    .Select(v => int.Parse(v.Trim()))
                    .ToArray();

                return Color.FromArgb(values[0], values[1], values[2]);
            }

            return Color.Black;
        }

        private static void ApplyStyles(Paragraph element, IReadOnlyDictionary<string, string> styles)
        {
            if (styles.ContainsKey("text-align"))
            {
                switch (styles["text-align"])
                {
                    case "left": element.Alignment = Element.ALIGN_LEFT; break;
                    case "right": element.Alignment = Element.ALIGN_RIGHT; break;
                    case "center": element.Alignment = Element.ALIGN_CENTER; break;
                }
            }
        }

        private static void ApplyStyles(Image element, IReadOnlyDictionary<string, string> styles)
        {
            if (styles.ContainsKey("float"))
            {
                switch (styles["float"])
                {
                    case "left": element.Alignment = Element.ALIGN_LEFT | Image.TEXTWRAP; break;
                    case "right": element.Alignment = Element.ALIGN_RIGHT | Image.TEXTWRAP; break;
                }
            }

            int height = 0;
            int width = 0;

            if (styles.ContainsKey("width"))
            {
                var attr = styles["width"];

                if (attr.EndsWith("px"))
                {
                    width = int.Parse(attr.Replace("px", ""));
                }
                else if (attr.EndsWith("%"))
                {
                    width = int.Parse(attr.Replace("%", ""));
                }
            }

            if (styles.ContainsKey("height"))
            {
                var attr = styles["height"];

                if (attr.EndsWith("px"))
                {
                    height = int.Parse(attr.Replace("px", ""));
                }
                else if (attr.EndsWith("%"))
                {
                    height = int.Parse(attr.Replace("%", ""));
                }
            }

            element.ScaleAbsolute(width, height);
        }

        private void ApplyStyles(PdfPCell element, Dictionary<string, string> styles)
        {
            if (styles.ContainsKey("text-align"))
            {
                switch (styles["text-align"])
                {
                    case "left": element.HorizontalAlignment = Element.ALIGN_LEFT; break;
                    case "right": element.HorizontalAlignment = Element.ALIGN_RIGHT; break;
                    case "center": element.HorizontalAlignment = Element.ALIGN_CENTER; break;
                }
            }

            if (styles.ContainsKey("vertical-align"))
            {
                switch (styles["vertical-align"])
                {
                    case "top": element.VerticalAlignment = Element.ALIGN_TOP; break;
                    case "bottom": element.VerticalAlignment = Element.ALIGN_BOTTOM; break;
                    case "middle": element.VerticalAlignment = Element.ALIGN_MIDDLE; break;
                    case "baseline": element.VerticalAlignment = Element.ALIGN_BASELINE; break;
                }
            }

            if (styles.ContainsKey("height"))
            {
                var value = styles["height"];
                value = value.Replace("px", "");

                element.FixedHeight = int.Parse(value);
            }
        }


        private static Dictionary<string, string> ParseStyles(string attribute)
        {
            var style = attribute.Split(';');
            return style
                .Select(rule => rule.Split(':'))
                .Where(rule => rule.Count() == 2)
                .ToDictionary(
                        parts => parts[0].Trim().ToLower(),
                        parts => parts[1].Trim().ToLower()
                );
        }

        private static String GetAttributeValue(HtmlNode node, string name)
        {
            var colspanAttr = node.Attributes.FirstOrDefault(a => a.Name == name);
            return colspanAttr != null ? colspanAttr.Value : "";
        }

        private static Font GetFont(FontStyle style)
        {
            var font = new Font(PdfFonts.Normal);
            font.SetStyle(Font.NORMAL
                          | (style.Bold ? Font.BOLD : 0)
                          | (style.Italic ? Font.ITALIC : 0)
                          | (style.Underlined ? Font.UNDERLINE : 0)
                );

            return font;
        }

        private class TableState
        {
            public TableState()
            {
                Cells = new List<PdfPCell>();
                ColumnWidths = new List<int>();
            }

            public List<PdfPCell> Cells { get; private set; }
            public List<int> ColumnWidths { get; private set; }
            public bool ColumnsCounted { get; set; }
            public int ColumnCount { get; set; }
            public int BorderWidth { get; set; }
            public int CellSpacing { get; set; }
            public int CellPadding { get; set; }
        }

        private class DocumentState
        {
            public DocumentState(FontStyle font)
            {
                Font = font;
                ListLevel = 1;
            }

            public int ListLevel { get; set; }
            public FontStyle Font { get; private set; }
        }

        private class FontStyle
        {
            public bool Bold { get; set; }
            public bool Italic { get; set; }
            public bool Underlined { get; set; }
        }


        private class PdfElementStore : ITextElementArray
        {
            public readonly List<IElement> Elements;

            public PdfElementStore()
            {
                Elements = new List<IElement>();
            }

            public bool Process(IElementListener listener)
            {
                throw new NotImplementedException();
            }

            public bool IsContent()
            {
                return true;
            }

            public bool IsNestable()
            {
                return true;
            }

            public int Type { get; private set; }
            public IList<Chunk> Chunks { get; private set; }

            public bool Add(IElement element)
            {
                Elements.Add(element);
                return true;
            }
        }
    }
}