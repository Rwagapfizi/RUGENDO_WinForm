namespace RUGENDO_WinForm.Controllers;

/// <summary>FR-04: a worker can only see their own company.</summary>
public class CompanyController
{
    readonly CompanyRepository _companies = new();

    public Result<Company> GetMyCompany() => Safe.Run(() =>
    {
        var w = Session.Worker;
        if (w == null) return Result<Company>.Fail("Only workers can view company details.");
        var co = _companies.GetById(w.CompanyId!.Value);
        return co == null ? Result<Company>.Fail("Company not found.") : Result<Company>.Success(co);
    });
}
