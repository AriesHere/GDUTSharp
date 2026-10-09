namespace GDUTSharp.Shared.Type.DTO;

#pragma warning disable IDE1006 // Naming Styles

public class SemesterRegDtoCollection : DtoCollectionBase<SemesterReg, SemesterRegDto>
{
    public override List<SemesterReg> Convert() => [..this];
}

public class SemesterRegDto
{
    /// <summary>学年学期名称</summary>
    public string xnxqmc { get; set; } = string.Empty;

    /// <summary>注册状态</summary>
    public string zczt { get; set; } = string.Empty;

    public static implicit operator SemesterReg(SemesterRegDto dto)
    {
        SemesterReg r = new()
        {
            Status = dto.zczt,
        };
        if (Term.TryParse(dto.xnxqmc, out var term))
        {
            r.Term = term;
        }
        return r;
    }
}

#pragma warning restore IDE1006 // Naming Styles
