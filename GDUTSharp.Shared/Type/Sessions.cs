namespace GDUTSharp.Shared.Type;

public partial class SessionCollection : List<Session>
{
    public bool Check()
    {
        TimeOnly prev = TimeOnly.MinValue;
        foreach (var period in this)
        {
            if (period.StartTime < prev || period.EndTime < period.StartTime)
            {
                return false;
            }
            prev = period.EndTime;
        }
        return true;
    }

    public static readonly SessionCollection Default =
        [
            new(new TimeOnly(8, 30), new TimeOnly(9, 15)),
            new(new TimeOnly(9, 20), new TimeOnly(10, 05)),
            new(new TimeOnly(10, 25), new TimeOnly(11, 10)),
            new(new TimeOnly(11, 15), new TimeOnly(12, 00)),
            new(new TimeOnly(13, 50), new TimeOnly(14, 35)),
            new(new TimeOnly(14, 40), new TimeOnly(15, 25)),
            new(new TimeOnly(15, 30), new TimeOnly(16, 15)),
            new(new TimeOnly(16, 30), new TimeOnly(17, 15)),
            new(new TimeOnly(17, 20), new TimeOnly(18, 05)),
            new(new TimeOnly(18, 30), new TimeOnly(19, 15)),
            new(new TimeOnly(19, 20), new TimeOnly(20, 05)),
            new(new TimeOnly(20, 10), new TimeOnly(20, 55)),
        ];
}

public struct Session(TimeOnly start, TimeOnly end)
{
    public TimeOnly StartTime = start;
    public TimeOnly EndTime = end;
    public readonly TimeSpan Duration => EndTime - StartTime;
}
