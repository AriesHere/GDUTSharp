using GDUTSharp.Shared.Type;

namespace GDUTSharp.Test;

[TestClass]
public sealed class TermTest
{
    [TestMethod]
    [DataRow("2026春季", 202502)]
    [DataRow("2026秋季", 202601)]
    [DataRow("2027春季", 202602)]
    [DataRow("2027秋季", 202701)]
    [DataRow("202501", 202501)]
    [DataRow("202502", 202502)]
    [DataRow("20251", 202501)]
    [DataRow("20252", 202502)]
    [DataRow(20251, 202501)]
    [DataRow(20252, 202502)]
    // 注意 expected6 是六位数字
    public void TestTermParse(object raw, int expected6)
    {
        Term t;
        if (raw is string s)
        {
            t = new(s);
        }
        else if (raw is int i)
        {
            t = new(i);
        }
        else
        {
            throw new InvalidDataException("raw 的类型必须是 string 或 int");
        }
        Assert.AreEqual(expected6, t.Code6);
    }

    [TestMethod]
    [DataRow(202501, true, 202502)]
    [DataRow(202501, false, 202402)]
    [DataRow(202502, true, 202601)]
    // 注意 expected6 是六位数字
    public void TestNextPrev(int raw, bool isNext, int expected6)
    {
        Term t = new(raw);
        int result = (isNext ? t.Next() : t.Prev()).Code6;
        Assert.AreEqual(expected6, result);
    }

    [TestMethod]
    [DataRow(202502, "2026春季")]
    [DataRow(202601, "2026秋季")]
    [DataRow(202602, "2027春季")]
    [DataRow(202701, "2027秋季")]
    public void TestTermName(int raw, string expected)
    {
        Term t = new(raw);
        Assert.AreEqual(expected, t.Name);
    }
}
