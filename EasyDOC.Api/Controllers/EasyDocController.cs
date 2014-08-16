using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.OData.Query;
using Breeze.WebApi;
using EasyDOC.BLL.Services;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;
using EasyDOC.Pdf.ManualTableGenerators;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json.Linq;
using WebMatrix.WebData;
using File = EasyDOC.Model.File;
using SaveResult = Breeze.ContextProvider.SaveResult;
using Task = EasyDOC.Model.Task;

namespace EasyDOC.Api.Controllers
{
    [System.Web.Http.Authorize]
    [BreezeController]
    [BreezeQueryable(AllowedArithmeticOperators = AllowedArithmeticOperators.All, AllowedLogicalOperators = AllowedLogicalOperators.All, MaxAnyAllExpressionDepth = 10, AllowedQueryOptions = AllowedQueryOptions.All, AllowedFunctions = AllowedFunctions.AllFunctions, MaxNodeCount = 9999, MaxTop = 100)]
    public class EasyDocController : ApiController
    {
        protected readonly ManualContextProvider ContextProvider = new ManualContextProvider();

        [HttpGet]
        public string Metadata()
        {
            return ContextProvider.Metadata();
        }

        public object GetPermissions()
        {
            var uow = new UnitOfWork();
            User user = uow.UserRepository.GetByKey(WebSecurity.CurrentUserId);

            if (user == null)
                throw new Exception("Unknown user id");

            return new
            {
                fileRoles = FileRoles(),
                permissions = user.Roles.SelectMany(r => r.Role.Permissions).AsQueryable(),
                projectStatus = ProjectStatus(),
                roleScopes = RoleScopes(),
                safetyRoles = SafetyRoles(),
                taskConstraints = TaskConstraints(),
                taskLinkTypes = TaskLinkTypes(),
                taskStates = TaskStates(),
                units = Units(),
                user
            };
        }

        [HttpGet]
        public IQueryable<Project> Projects()
        {
            return GetQueryable(ContextProvider.Context.Projects, "Project");
        }

        [HttpGet]
        public IQueryable<ProjectFile> ProjectFiles()
        {
            return GetQueryable(ContextProvider.Context.ProjectFiles, "ProjectFile");
        }

        [HttpGet]
        public IQueryable<ProjectComponent> ProjectComponents()
        {
            return GetQueryable(ContextProvider.Context.ProjectComponents, "ProjectComponent");
        }

        [HttpGet]
        public IQueryable<ProjectMaintenance> ProjectMaintenances()
        {
            return GetQueryable(ContextProvider.Context.ProjectMaintenances, "ProjectMaintenance");
        }

        [HttpGet]
        public IQueryable<ProjectSafety> ProjectSafeties()
        {
            return GetQueryable(ContextProvider.Context.ProjectSafeties, "ProjectSafety");
        }

        [HttpGet]
        public IQueryable<Component> Components()
        {
            return GetQueryable(ContextProvider.Context.SingleComponents, "Component");
        }

        [HttpGet]
        public IQueryable<ComponentSeries> ComponentSeries()
        {
            return GetQueryable(ContextProvider.Context.ComponentSeries, "ComponentSeries");
        }

        [HttpGet]
        public IQueryable<ComponentFile> ComponentFiles()
        {
            return GetQueryable(ContextProvider.Context.ComponentFiles, "ComponentFile");
        }

        [HttpGet]
        public IQueryable<ComponentMaintenance> ComponentMaintenances()
        {
            return GetQueryable(ContextProvider.Context.ComponentMaintenances, "ComponentMaintenance");
        }

        [HttpGet]
        public IQueryable<ComponentSafety> ComponentSafeties()
        {
            return GetQueryable(ContextProvider.Context.ComponentSafeties, "ComponentSafety");
        }

        [HttpGet]
        public IQueryable<Category> Categories()
        {
            return GetQueryable(ContextProvider.Context.Categories, "Category");
        }

        [HttpGet]
        public IQueryable<Customer> Customers()
        {
            return GetQueryable(ContextProvider.Context.Customers, "Customer");
        }

        [HttpGet]
        public IQueryable<Documentation> Documentations()
        {
            return GetQueryable(ContextProvider.Context.Documentations, "Documentation");
        }

        [HttpGet]
        public IQueryable<DocumentationChapter> DocumentationChapters()
        {
            return GetQueryable(ContextProvider.Context.DocumentationChapters, "DocumentationChapter");
        }

        [HttpGet]
        public IQueryable<Employee> Employees()
        {
            return GetQueryable(ContextProvider.Context.Employees, "Employee");
        }

        [HttpGet]
        public IQueryable<EmployeeTask> EmployeeTasks()
        {
            RolePermission permission = GetUserPermission(WebSecurity.CurrentUserId, "EmployeeTask");
            return ContextProvider.Context.EmployeeTasks.Where(p => permission.Read == RoleScope.All);
        }

        [HttpGet]
        public IQueryable<Experience> Experiences()
        {
            return GetQueryable(ContextProvider.Context.Experiences, "Experience");
        }

        [HttpGet]
        public IQueryable<FileRole> FileRoles()
        {
            return ContextProvider.Context.FileRoles;
        }

        [HttpGet]
        public IQueryable<SafetyRole> SafetyRoles()
        {
            return ContextProvider.Context.SafetyRoles;
        }

        [HttpGet]
        public IQueryable<File> Files()
        {
            return GetQueryable(ContextProvider.Context.Files, "File");
        }

        [HttpGet]
        public IQueryable<Folder> Folders()
        {
            return GetQueryable(ContextProvider.Context.Folders, "Folder");
        }

        [HttpGet]
        public IQueryable<Maintenance> Maintenances()
        {
            return GetQueryable(ContextProvider.Context.Maintenances, "Maintenance");
        }

        [HttpGet]
        public IQueryable<Safety> Safeties()
        {
            return GetQueryable(ContextProvider.Context.Safeties, "Safety");
        }

        [HttpGet]
        public IQueryable<Task> Tasks()
        {
            return GetQueryable(ContextProvider.Context.Tasks, "Task");
        }

        [HttpGet]
        public IQueryable<TaskCheckPoint> TaskCheckPoints()
        {
            return GetQueryable(ContextProvider.Context.TaskCheckPoints, "TaskCheckPoint");
        }

        [HttpGet]
        public IQueryable<TaskLink> TaskLinks()
        {
            return GetQueryable(ContextProvider.Context.TaskLinks, "TaskLink");
        }

        [HttpGet]
        public IQueryable<TaskLinkType> TaskLinkTypes()
        {
            return ContextProvider.Context.TaskLinkTypes;
        }

        [HttpGet]
        public IQueryable<TaskState> TaskStates()
        {
            return ContextProvider.Context.TaskStates;
        }

        [HttpGet]
        public IQueryable<ProjectStatus> ProjectStatus()
        {
            return ContextProvider.Context.ProjectStatus;
        }

        [HttpGet]
        public IQueryable<RoleScope> RoleScopes()
        {
            return ContextProvider.Context.RoleScopes;
        }

        [HttpGet]
        public IQueryable<Unit> Units()
        {
            return ContextProvider.Context.Units;
        }

        [HttpGet]
        public IQueryable<TaskConstraint> TaskConstraints()
        {
            return ContextProvider.Context.TaskConstraints;
        }

        [HttpGet]
        public IQueryable<User> Users()
        {
            RolePermission permission = GetUserPermission(WebSecurity.CurrentUserId, "User");
            return ContextProvider.Context.Users.Where(p => permission.Read == RoleScope.All);
        }

        [HttpGet]
        public IQueryable<Role> Roles()
        {
            return GetQueryable(ContextProvider.Context.Roles, "Role");
        }

        [HttpGet]
        public IQueryable<OrderConfirmation> OrderConfirmations()
        {
            return GetQueryable(ContextProvider.Context.OrderConfirmations, "OrderConfirmation");
        }

        [HttpGet]
        public IQueryable<OrderConfirmationItem> OrderConfirmationItems()
        {
            return GetQueryable(ContextProvider.Context.OrderConfirmationItems, "OrderConfirmationItem");
        }

        [HttpGet]
        public IQueryable<Permission> Permissions()
        {
            return GetQueryable(ContextProvider.Context.Permissions, "Permission");
        }

        [HttpGet]
        public IQueryable<UserRole> UserRoles()
        {
            return GetQueryable(ContextProvider.Context.UserRoles, "UserRole");
        }

        [HttpGet]
        public IQueryable<RolePermission> RolePermissions()
        {
            return GetQueryable(ContextProvider.Context.RolePermissions, "RolePermission");
        }

        [HttpGet]
        public IQueryable<Vendor> Vendors()
        {
            return GetQueryable(ContextProvider.Context.Vendors, "Vendor");
        }

        [HttpGet]
        public IQueryable<AbstractChapter> AllChapters()
        {
            return GetQueryable(ContextProvider.Context.Chapters, "Chapter");
        }

        [HttpGet]
        public IQueryable<AbstractComponent> AllComponents()
        {
            return GetQueryable(ContextProvider.Context.Components, "Component");
        }

        [HttpGet]
        public IQueryable<Chapter> Chapters()
        {
            return GetQueryable(ContextProvider.Context.Chapters.OfType<Chapter>(), "Chapter");
        }

        [HttpGet]
        public IQueryable<ChapterFile> ChapterFiles()
        {
            return GetQueryable(ContextProvider.Context.ChapterFiles, "ChapterFile");
        }

        [HttpGet]
        public IQueryable<GeneratedChapter> GeneratedChapters()
        {
            return GetQueryable(ContextProvider.Context.Chapters.OfType<GeneratedChapter>(), "Chapter");
        }

        [HttpGet]
        public IQueryable<PdfChapter> PdfChapters()
        {
            return GetQueryable(ContextProvider.Context.Chapters.OfType<PdfChapter>(), "Chapter");
        }

        private IQueryable<T> GetQueryable<T>(IQueryable<T> type, string permissionName) where T : class, IHistoryEntity
        {
            RolePermission permission = GetUserPermission(WebSecurity.CurrentUserId, permissionName);
            return type.Where(p => (permission.Read == RoleScope.All || (p.CreatorId == WebSecurity.CurrentUserId && permission.Read == RoleScope.Owned)));
        }

        [HttpPost]
        public SaveResult SaveChanges(JObject saveBundle)
        {
            SaveResult result = ContextProvider.SaveChanges(saveBundle);
            IHubContext hub = GlobalHost.ConnectionManager.GetHubContext<EntityHub>();
            hub.Clients.All.entitiesChanged();
            return result;
        }

        [HttpGet]
        public HttpResponseMessage ExportPdf(int id = 0)
        {
            var uow = new UnitOfWork(ContextProvider.EntityConnection);
            Documentation documentation = uow.DocumentationRepository.GetByKey(id);

            var pdf = new DocumentationPdfCreator(documentation, new AbstractChapterService(null, uow, uow.ChapterRepository), HttpContext.Current.ApplicationInstance.Server.MapPath("/"));

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(pdf.GetPdf())
            };

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            return result;
        }

        private RolePermission GetUserPermission(int userId, string permissionName)
        {
            var uow = new UnitOfWork(ContextProvider.EntityConnection);
            User user = uow.UserRepository.GetByKeyNoTracking(userId);
            RolePermission permission = user.Roles.SelectMany(r => r.Role.Permissions).SingleOrDefault(p => p.Permission.Name == permissionName);

            return permission ?? new RolePermission();
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Upload()
        {
            string root = HttpContext.Current.Server.MapPath("~/");
            var provider = new MultipartFormDataStreamProvider(root);
            var newFiles = new List<string>();

            try
            {
                var uow = new UnitOfWork(ContextProvider.EntityConnection);
                RolePermission permission = GetUserPermission(WebSecurity.CurrentUserId, "File");

                // Check if the request contains multipart/form-data.
                if (!Request.Content.IsMimeMultipartContent())
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

                if (permission == null || permission.Create != RoleScope.All)
                    throw new HttpResponseException(HttpStatusCode.Forbidden);

                // Read the form data.
                MultipartFormDataStreamProvider form = await Request.Content.ReadAsMultipartAsync(provider);
                int id = Int32.Parse(form.FormData["folderId"]);

                var fileService = new FileService(null, uow);
                var folderService = new CatalogService(null, uow);
                Folder folder = folderService.GetByKey(id);

                if (folder == null)
                    throw new HttpResponseException(HttpStatusCode.NotFound);

                // This illustrates how to get the file names.
                foreach (MultipartFileData fileData in provider.FileData)
                {
                    string filename = fileData.Headers.ContentDisposition.FileName.Replace("\"", "");

                    string type = filename.Split('.').Last();
                    string name = filename.Substring(0, filename.LastIndexOf(type, StringComparison.Ordinal) - 1);

                    string newFile = root + folder.GetWindowsPath() + '\\' + filename;

                    var info = new FileInfo(fileData.LocalFileName);
                    long size = info.Length;

                    if (System.IO.File.Exists(newFile))
                    {
                        File existingFile = uow.FileRepository.Get(f => f.Name.ToLower() == name.ToLower() && f.Type.ToLower() == type.ToLower() && f.CatalogId == id).SingleOrDefault();

                        if (existingFile != null)
                            if (existingFile.IsOverwritable)
                            {
                                existingFile.IsOverwritable = false;
                                fileService.Update(existingFile);
                                System.IO.File.Copy(fileData.LocalFileName, newFile, true);
                                existingFile.Edited = DateTime.Now;
                            }
                            else
                                throw new HttpResponseException(HttpStatusCode.Found);
                    }
                    else
                    {
                        if (uow.FileRepository.Get(f => f.Name.ToLower() == name.ToLower() && f.Type == type.ToLower()).Any())
                            throw new HttpResponseException(HttpStatusCode.Ambiguous);

                        System.IO.File.Copy(fileData.LocalFileName, newFile);
                        newFiles.Add(newFile);

                        var file = new File
                        {
                            CatalogId = id,
                            Name = name,
                            FileSize = (int) size,
                            Type = type.ToLower(),
                            CreatorId = WebSecurity.CurrentUserId
                        };

                        fileService.Create(file);
                    }
                }

                uow.Commit();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception)
            {
                // Delete all files if an exception is thrown. This enables us to upload the files anew.
                foreach (string file in newFiles)
                {
                    System.IO.File.Delete(file);
                }

                throw;
            }
            finally
            {
                // Delete temporary files 
                foreach (MultipartFileData fileData in provider.FileData)
                {
                    System.IO.File.Delete(fileData.LocalFileName);
                }
            }
        }
    }

    public class EntityHub : Hub {}
}