using System.Text.Json.Serialization;
using GDUTSharp.Shared.Attributes;

namespace GDUTSharp.Shared.Type;

/// <summary>学期</summary>
public class Term
{
    public int Year { get; set; }

    public TermPeriod Period { get; set; }

    public Term() { }

    public Term(int year, TermPeriod period)
    {
        Year = year;
        Period = period;
    }

    public Term(int value) : this(value.ToString()) { }

    public Term(string value)
    {
        if (value.Length is not (5 or 6)) throw new InvalidCastException("预料之外的字符串长度");
        Year = int.Parse(value[0..4]);
        var r = Enum.TryParse<TermPeriod>(value[4..^0], out var period);
        if (r) Period = period;
        else
        {
            switch(value[4])
            {
                case '春':
                    Year--;
                    Period = TermPeriod.Second;
                    break;
                case '秋':
                    Period = TermPeriod.First;
                    break;
                default: throw new InvalidCastException($"预料之外的字符：{value[5]}");
            };
        }
    }

    [JsonIgnore]
    /// <summary>获取学期名称</summary>
    public string Name => Period == TermPeriod.First ? $"{Year}秋季" : $"{++Year}春季";

    [JsonIgnore]
    /// <summary>获取 5 位学期代码，常用于研究生部分</summary>
    public int Code5 => int.Parse($"{Year}{(int)Period}");

    [JsonIgnore]
    /// <summary>获取 6 位学期代码，常用于本科生部分</summary>
    public int Code6 => int.Parse($"{Year}0{(int)Period}");

    /// <summary>通过今天获取学期（可能会与学校那边有出入）</summary>
    public static Term FromToday()
    {
        var dt = DateTime.Today;
        return (dt.Month < 3 || dt.Month >= 9) ? new(dt.Year, TermPeriod.First) : new(dt.Year - 1, TermPeriod.Second);
    }

    public static Term Parse(string value) => new(value);

    public Term Next()
    {
        Term t = new(this.Year, this.Period);
        switch (t.Period)
        {
            case TermPeriod.First:
                t.Period = TermPeriod.Second;
                break;
            case TermPeriod.Second:
                t.Year++;
                t.Period = TermPeriod.First;
                break;
            default: throw new InvalidCastException();
        }
        return t;
    }

    public Term Prev()
    {
        Term t = new(this.Year, this.Period);
        switch (t.Period)
        {
            case TermPeriod.First:
                t.Year--;
                t.Period = TermPeriod.Second;
                break;
            case TermPeriod.Second:
                t.Period = TermPeriod.First;
                break;
            default: throw new InvalidCastException();
        }
        return t;
    }

    public override string ToString() => this.Name;
}

public enum TermPeriod
{
    First = 1,
    Second = 2,
}
