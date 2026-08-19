namespace GDUTSharp.Shared.Type.DTO;

#pragma warning disable IDE1006 // Naming Styles

public class DtoCollectionBase<T>
{
    public List<T> rows { get; set; } = [];

    public int Count => rows.Count;

    public void Add(T item) => rows.Add(item);

    public void Clear() => rows.Clear();

    public void CopyTo(T[] array, int arrayIndex) => rows.CopyTo(array, arrayIndex);

    public IEnumerator<T> GetEnumerator() => rows.GetEnumerator();

    public T this[int index] => rows[index];
}

#pragma warning restore IDE1006 // Naming Styles