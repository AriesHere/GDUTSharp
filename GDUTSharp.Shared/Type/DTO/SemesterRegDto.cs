namespace GDUTSharp.Shared.Type.DTO;

#pragma warning disable IDE1006 // Naming Styles

public class SemesterRegDtoCollection : DtoCollectionBase<SemesterRegDto>
{
    public static implicit operator List<SemesterReg>(SemesterRegDtoCollection? collection) => collection is null ? [] : [.. collection];
}

public class SemesterRegDto
{
    /// <summary>学年学期名称</summary>
    public string xnxqmc { get; set; } = string.Empty;

    /// <summary>注册状态</summary>
    public string zczt { get; set; } = string.Empty;

    public static implicit operator SemesterReg(SemesterRegDto dto)
    {
        return new SemesterReg
        {
            Term = dto.xnxqmc,
            Status = dto.zczt,
        };
    }
}

#pragma warning restore IDE1006 // Naming Styles
