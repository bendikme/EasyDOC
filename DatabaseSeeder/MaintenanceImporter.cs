using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Text;
using EasyDOC.BLL.Services;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace DatabaseSeeder
{
    class MaintenanceImporter
    {
        private readonly string _filename;

        public MaintenanceImporter(string filename)
        {
            _filename = filename;
        }

        private readonly Dictionary<string, string> _vendorAbbr = new Dictionary<string, string>
        {
            {"ANR", "Anritsu"},
            {"ALF", "Alfa Laval"},
            {"DYN", "Dynatec"},
            {"DTC", "Dtc Lenze"},
            {"DOS", "Dosetec"},
            {"ENP", "ENP"},
            {"HOL", "Dynatec"},
            {"ILA", "Ilapak"},
            {"IMA", "Imaje"},
            {"ISH", "Ishida"},
            {"LUN", "Lundgrens"},
            {"MIT", "Mitsubishi"},
            {"KÖN", "König"},
            {"MIX", "Mixer"},
            {"OMR", "Omron"},
            {"SEW", "SEW"},
            {"SAN", "Sancassiano"},
            {"SPA", "Spantech"},
            {"TEC", "Technopool"},
            {"TEP", "Tepro"}
        };

        private List<Vendor> _vendors = null; 

        private Vendor FindOrCreateVendor(string abbr, IUnitOfWork uow)
        {
            if (_vendorAbbr.ContainsKey(abbr))
            {
                if (_vendors == null)
                {
                    _vendors = uow.VendorRepository.GetAll().ToList();
                }

                var vendor = _vendors.SingleOrDefault(v => v.Name.StartsWith(_vendorAbbr[abbr], true, CultureInfo.InvariantCulture));

                if (vendor == null)
                {
                    vendor = new Vendor
                    {
                        Name = _vendorAbbr[abbr]
                    };

                    var service = new VendorService(null, uow);
                    service.Create(vendor);
                    _vendors.Add(vendor);
                }

                return vendor;
            }

            return null;
        }

        public void Execute()
        {
            var uow = new UnitOfWork();
            var service = new MaintenanceService(null, uow);
            var text = System.IO.File.ReadAllText(_filename, Encoding.Default);

            var maintenances = service.GetAll();

            var lines = text.Split(new[] { "\r\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                var columns = line.Split(';');

                if (columns.Length == 20)
                {

                    var titleNo = columns[0].Trim();
                    var descNo = columns[1].Trim();
                    var titleSe = columns[3].Trim();
                    var descSe = columns[4].Trim();

                    var daily = columns[7].Trim() == "X";
                    var weekly = columns[8].Trim() == "X";
                    var weekly2 = columns[9].Trim() == "X";
                    var monthly = columns[10].Trim() == "X";
                    var monthly3 = columns[11].Trim() == "X";
                    var monthly6 = columns[12].Trim() == "X";
                    var yearly = columns[13].Trim() == "X";
                    var rarely = columns[14].Trim() == "X";

                    var vendor = columns[19];

                    Maintenance maintenanceNo = null;

                    if (titleNo.Length > 0)
                    {
                        maintenanceNo = maintenances.SingleOrDefault(m => m.Name.StartsWith(titleNo)) ?? new Maintenance();

                        maintenanceNo.Name = titleNo;
                        maintenanceNo.Description = descNo;
                        maintenanceNo.IntervalDaily = daily;
                        maintenanceNo.IntervalWeekly = weekly;
                        maintenanceNo.IntervalWeekly2 = weekly2;
                        maintenanceNo.IntervalMonthly = monthly;
                        maintenanceNo.IntervalMonthly3 = monthly3;
                        maintenanceNo.IntervalHalfYearly = monthly6;
                        maintenanceNo.IntervalYearly = yearly;
                        maintenanceNo.IntervalRarely = rarely;
                        maintenanceNo.LanguageId = 1;
                        maintenanceNo.Vendor = FindOrCreateVendor(vendor, uow);

                        service.Create(maintenanceNo);
                    }

                    if (titleSe.Length > 0)
                    {
                        var maintenanceSe = new Maintenance
                        {
                            Name = titleSe,
                            Description = descSe,
                            IntervalDaily = daily,
                            IntervalWeekly = weekly,
                            IntervalWeekly2 = weekly2,
                            IntervalMonthly = monthly,
                            IntervalMonthly3 = monthly3,
                            IntervalHalfYearly = monthly6,
                            IntervalYearly = yearly,
                            IntervalRarely = rarely,
                            TranslatedFrom = maintenanceNo,
                            LanguageId = 2,
                            Vendor = FindOrCreateVendor(vendor, uow)
                        };

                        service.Create(maintenanceSe);

                        if (maintenanceNo != null)
                        {
                            maintenanceNo.Translations.Add(maintenanceSe);
                        }

                    }
                }
            }

            try
            {
                uow.Commit();
            }
            catch (DbEntityValidationException e)
            {

            }
            Console.ReadKey();
        }
    }
}