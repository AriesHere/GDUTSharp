namespace GDUTSharp.Shared.Type.DTO;

#pragma warning disable IDE1006 // Naming Styles

public abstract class DtoCollectionBase<TResult, TDto>
{
    public List<TDto> rows { get; set; } = [];

    public int Count => rows.Count;

    public void Add(TDto item) => rows.Add(item);

    public IEnumerator<TDto> GetEnumerator() => rows.GetEnumerator();

    public abstract List<TResult> Convert();
}

#pragma warning restore IDE1006 // Naming Styles