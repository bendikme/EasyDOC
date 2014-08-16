using EasyDOC.Model;

namespace EasyDOC.BLL.Services
{
    public interface IDocumentationService : IGenericService<Documentation>
    {
        void AddChapter(Documentation documentation, int chapterId, string chapter, string parameters, int? fileId);
        void RemoveChapter(Documentation documentation, int chapterId, string chapterNumber);
    }
}