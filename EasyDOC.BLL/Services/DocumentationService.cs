using System.Linq;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace EasyDOC.BLL.Services
{
    public class DocumentationService : GenericService<Documentation>, IDocumentationService
    {
        public DocumentationService(IValidationDictionary validationDictionary, IUnitOfWork unitOfWork)
            : base(validationDictionary, unitOfWork, unitOfWork.DocumentationRepository) { }

        public void AddChapter(Documentation documentation, int chapterId, string chapter, string parameters, int? fileId)
        {
            var item = fileId.HasValue ? 
                UnitOfWork.AllChaptersRepository.Get(c => c is PdfChapter).Single() : 
                UnitOfWork.AllChaptersRepository.GetByKey(chapterId);

            var relation = new DocumentationChapter
            {
                Documentation = documentation,
                Chapter = item,
                ChapterNumber = chapter,
                Parameters = parameters,
				FileId = fileId
            };

            item.DocumentationChapters.Add(relation); 
        }

        public void RemoveChapter(Documentation documentation, int chapterId, string chapterNumber)
        {
            var child = documentation.DocumentationChapters.Single(c => c.ChapterId == chapterId && c.ChapterNumber == chapterNumber);
            documentation.DocumentationChapters.Remove(child);
        }
    }
}