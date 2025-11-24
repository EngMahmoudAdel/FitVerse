using AutoMapper;
using FitVerse.Core.IUnitOfWorkServices;
using FitVerse.Core.UnitOfWork;
using FitVerse.Core.ViewModels.Package;
using FitVerse.Data.Models;

public class PackageAppService : IPackageAppService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;

    public PackageAppService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
        
    }

    public IEnumerable<PackageVM> GetPaged(int page, int pageSize, string search, out int totalPages)
    {
        var query = unitOfWork.Packages.GetAll();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search));

        var totalItems = query.Count();
        totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        return query
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => mapper.Map<PackageVM>(p))
            .ToList();
    }

    public PackageVM? GetById(int id)
    {
        var pkg = unitOfWork.Packages.GetById(id);
        return pkg == null ? null : mapper.Map<PackageVM>(pkg);
    }

    public bool Create(AddPackageVM package, out string message)
    {
        var pkg = mapper.Map<Package>(package);
        unitOfWork.Packages.Add(pkg);

        if (unitOfWork.Complete() > 0)
        {
            message = "Package created successfully.";
            return true;
        }

        message = "Something went wrong while creating package.";
        return false;
    }

    public bool Update(PackageVM package, out string message)
    {
        var pkg = unitOfWork.Packages.GetById(package.Id);
        if (pkg == null)
        {
            message = "Package not found.";
            return false;
        }

        mapper.Map(package, pkg);
        unitOfWork.Packages.Update(pkg);

        if (unitOfWork.Complete() > 0)
        {
            message = "Package updated successfully.";
            return true;
        }

        message = "Something went wrong while updating package.";
        return false;
    }

    public bool Delete(int id, out string message)
    {
        var pkg = unitOfWork.Packages.GetById(id);
        if (pkg == null)
        {
            message = "Package not found.";
            return false;
        }

        unitOfWork.Packages.Delete(pkg);

        if (unitOfWork.Complete() > 0)
        {
            message = "Package deleted successfully.";
            return true;
        }

        message = "Something went wrong while deleting package.";
        return false;
    }

    public List<PackageVM> GetAllPackages()
    {
        var packages = unitOfWork.Packages.GetAll().ToList();
        return mapper.Map<List<PackageVM>>(packages);
    }
    public void AssignPackagesToCoach(string coachId, List<int> selectedPackageIds)
    {
        if (string.IsNullOrEmpty(coachId))
            throw new ArgumentNullException(nameof(coachId));

        var oldRelations = unitOfWork.coachPackageRepository
            .Find(cp => cp.CoachId == coachId);

        unitOfWork.coachPackageRepository.RemoveRange(oldRelations);

        if (selectedPackageIds != null && selectedPackageIds.Any())
        {
            foreach (var packageId in selectedPackageIds)
            {
                var coachPackage = new CoachPackage
                {
                    CoachId = coachId,
                    PackageId = packageId
                };
                unitOfWork.coachPackageRepository.Add(coachPackage);
            }
        }

        unitOfWork.coachPackageRepository.complete();
    }

   
}


