using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using EasyDOC.BLL.Services;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;
using Type = EasyDOC.Model.Type;

namespace DatabaseSeeder
{
    class ComponentImporter
    {
        private readonly string _filename;

        public ComponentImporter(string filename)
        {
            _filename = filename;
        }

        private readonly Dictionary<string, string> _vendorAbbr = new Dictionary<string, string>
        {
            {"NOR", "Norgren"},
            {"TEL", "Telematique"},
            {"BEI", "Beijer"},
            {"LIN", "Linak"},
            {"DYN", "Dynatec"},
            {"MUR", "Murr Elektronik"},
            {"DAN", "Danfoss"},
            {"FES", "Festo"},
            {"INT", "Intralox"},
            {"OTR", "Otra"},
            {"OMR", "Omron"},
            {"PMC", "PMC"},
            {"SIC", "Sick"},
            {"STE", "Steno"},
            {"AMM", "Ammercurve"},
            {"SPA", "Spantech"},
            {"REX", "Bosch Rexroth"},
            {"SEW", "SEW Eurodrive"},
            {"TEC", "Technopool"},
            {"STI", "Stige Senteret AS"},
            {"ALF", "Alfa Laval"},
            {"ALP", "Alpha Drives"},
            {"ANR", "Anritsu"},
            {"ILA", "Ilapak"},
            {"ISH", "Ishida"},
            {"KÖN", "König"},
            {"MIT", "Mitsubishi"},
            {"DTC", "Dtc Lenze"},
        };

        private static Category CreateLangCategory(Category category, int languageId, IUnitOfWork uow)
        {
            if (category == null)
            {
                return null;
            }

            var entity = uow.CategoryRepository.Get(c => c.Name.Equals(category.Name, StringComparison.OrdinalIgnoreCase) && c.LanguageId == languageId).FirstOrDefault();
            if (entity != null)
            {
                return entity;
            }

            entity = new Category
            {
                Name = category.Name,
                LanguageId = languageId,
                TranslatedFrom = category
            };

            uow.CategoryRepository.Create(entity);
            uow.Commit();
            return entity;
        }

        private Type CreateLangType(string name, Type type, int languageId, IUnitOfWork uow)
        {
            if (type == null || name.Length == 0)
            {
                return null;
            }

            var entity = uow.TypeRepository.Get(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && c.LanguageId == languageId).FirstOrDefault();
            if (entity != null)
            {
                return entity;
            }

            entity = new Type
            {
                Name = name,
                LanguageId = languageId,
                TranslatedFrom = type
            };

            uow.TypeRepository.Create(entity);
            uow.Commit();
            return entity;
        }

        private static Category FindOrCreateCategory(string category, int languageId, IUnitOfWork uow)
        {
            var entity = uow.CategoryRepository.Get(c => c.Name.Equals(category, StringComparison.OrdinalIgnoreCase) && c.LanguageId == languageId).FirstOrDefault();
            if (entity != null || category.Length == 0)
            {
                return entity;
            }

            entity = new Category
            {
                Name = category,
                LanguageId = languageId
            };

            uow.CategoryRepository.Create(entity);
            uow.Commit();
            return entity;
        }

        private static Type FindOrCreateType(string type, int languageId, IUnitOfWork uow)
        {
            var entity = uow.TypeRepository.Get(c => c.Name.Equals(type, StringComparison.OrdinalIgnoreCase) && c.LanguageId == languageId).FirstOrDefault();
            if (entity != null || type.Length == 0)
            {
                return entity;
            }

            entity = new Type
            {
                Name = type,
                LanguageId = languageId
            };

            uow.TypeRepository.Create(entity);
            uow.Commit();
            return entity;
        }

        private Vendor FindOrCreateVendor(string abbr, IUnitOfWork uow)
        {
            if (_vendorAbbr.ContainsKey(abbr))
            {
                var vendors = uow.VendorRepository.GetAll().ToList();
                var vendor = vendors.FirstOrDefault(v => v.Name.StartsWith(_vendorAbbr[abbr]));

                if (vendor == null)
                {
                    vendor = new Vendor
                    {
                        Name = _vendorAbbr[abbr],
                        ShortName = abbr
                    };

                    uow.VendorRepository.Create(vendor);
                    uow.Commit();
                }
                else
                {
                    vendor.ShortName = abbr;
                    uow.VendorRepository.Update(vendor);
                    uow.Commit();
                }

                return vendor;
            }

            return null;
        }

        public void Execute()
        {
            var uow = new UnitOfWork();
            var service = new ComponentService(null, uow);

            var text = System.IO.File.ReadAllText(_filename, Encoding.Default);


            var lines = text.Split(new[] { "\r\n" }, StringSplitOptions.None);

            try
            {
                foreach (var line in lines)
                {
                    var columns = line.Split(';');

                    if (columns.Length == 8)
                    {

                        var category = CleanString(columns[0]);
                        var typeNo = CleanString(columns[2]);
                        var typeSe = CleanString(columns[3]);
                        var vendor = CleanString(columns[4]);
                        var article = CleanString(columns[5]);
                        var descNo = CleanString(columns[6]);
                        var descSe = CleanString(columns[7]);

                        if (article.Length > 0 && (category.Length > 1 || typeNo.Length > 1))
                        {
                            var components = service.GetAll();
                            var compNo = components.FirstOrDefault(c => c.Name == article && c.Description == descNo && c.LanguageId == 1);
                            var compSe = components.FirstOrDefault(c => c.Name == article && c.Description == descSe && c.LanguageId == 2);

                            if (compNo == null)
                            {
                                compNo = new Component
                                {
                                    Name = article,
                                    Description = descNo,
                                    Vendor = FindOrCreateVendor(vendor, uow),
                                    LanguageId = 1,
                                    Category = FindOrCreateCategory(category, 1, uow),
                                    Type = FindOrCreateType(typeNo, 1, uow)
                                };

                                service.Create(compNo);
                                uow.Commit();
                            }

                            if (compSe == null)
                            {

                                compSe = new Component
                                {
                                    Name = article,
                                    Description = descSe,
                                    Vendor = compNo.Vendor,
                                    LanguageId = 2,
                                    Category = CreateLangCategory(compNo.Category, 2, uow),
                                    Type = CreateLangType(typeSe, compNo.Type, 2, uow),
                                    TranslatedFrom = compNo
                                };

                                service.Create(compSe);
                                uow.Commit();
                            }
                        }
                    }
                }

            }
            catch (DbEntityValidationException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private string CleanString(string input)
        {
            input = input.Trim();
            if (input.StartsWith("\"") && input.EndsWith("\""))
            {
                return input.Substring(1, input.Length - 2).Trim();
            }

            return input;
        }
    }
}