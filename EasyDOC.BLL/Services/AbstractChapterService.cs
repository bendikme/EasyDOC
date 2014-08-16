using System;
using System.Linq;
using EasyDOC.DAL.DataAccess;
using EasyDOC.DAL.Repositories;
using EasyDOC.Model;
using HtmlAgilityPack;

namespace EasyDOC.BLL.Services
{
    public class AbstractChapterService : GenericService<Chapter>, IChapterService
    {
        public AbstractChapterService(IValidationDictionary validationDictionary, IUnitOfWork unitOfWork, IRepository<Chapter> repository)
            : base(validationDictionary, unitOfWork, repository) { }

        private static readonly Tuple<string, string>[] Checks = { new Tuple<string, string>("a", "href"), new Tuple<string, string>("img", "src") };

        public override Chapter GetByKey(int id)
        {
            var chapter = base.GetByKey(id);

            if (chapter != null)
            {
                chapter.Content = GetContent(chapter);
            }
            return chapter;
        }

        public override void Update(Chapter chapter)
        {
            var document = new HtmlDocument();
            document.LoadHtml(chapter.Content);

            chapter.Files.Clear();

            foreach (var check in Checks)
            {
                var links = document.DocumentNode.SelectNodes(string.Format("//{0}[@{1}]", check.Item1, check.Item2));

                if (links != null && links.Count > 0)
                {
                    foreach (var link in links)
                    {
                        var reference = link.Attributes[check.Item2].Value;
                        if (!reference.StartsWith("http://"))
                        {
                            var file = UnitOfWork.FileRepository
                                .GetAll()
                                .SingleOrDefault(f => f.GetPath().ToLower() == reference.ToLower());

                            if (file != null)
                            {
                                chapter.Files.Add(new ChapterFile
                                {
                                    Chapter = chapter,
                                    File = file
                                });
                                link.Attributes[check.Item2].Value = string.Format("{0}", file.Id);
                            }
                        }
                    }
                }
            }

            chapter.Content = document.DocumentNode.OuterHtml;
            Repository.Update(chapter);
        }

        public string GetContent(AbstractChapter chapter)
        {
            var document = new HtmlDocument();

            if (chapter != null && chapter.Content != null)
            {
                document.LoadHtml(chapter.Content);

                foreach (var check in Checks)
                {
                    var links = document.DocumentNode.SelectNodes(string.Format("//{0}[@{1}]", check.Item1, check.Item2));

                    if (links != null && links.Count > 0)
                    {
                        foreach (var link in links)
                        {
                            int fileId;

                            if (int.TryParse(link.Attributes[check.Item2].Value, out fileId))
                            {
                                var file = UnitOfWork.FileRepository.GetByKey(fileId);
                                if (file != null) link.Attributes[check.Item2].Value = file.GetPath();
                            }
                        }
                    }
                }

                return document.DocumentNode.OuterHtml;
            }

            return "";
        }
    }
}