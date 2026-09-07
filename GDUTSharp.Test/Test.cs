using GDUTSharp.Extra;
using GDUTSharp.Shared;

namespace GDUTSharp.Test
{
    [TestClass]
    public sealed class Test
    {
        [TestMethod]
        public void TestJXFWExportedTEXTLessonsFileAnalysis()
        {
            var raw = """
                "课程名称","班级名称","人数","教师","周次","星期","节次","上课地点","排课日期","课序","类型","授课内容简介",
                "劳动教育","某某班级(1),某某班级(2)","60","某某老师","1","4","08","某上课地点","2026-09-07","1","实验教学","测试简介"
                "大学美育(1)","某某班级(1),某某班级(2),某某班级(3),某某班级(4)","98","某某老师","1","1","101112","某上课地点","2026-09-07","1","理论教学","简介内容"
                """;
            MemoryStream s = new(raw.GetBytes());
            var r = ExtraExtensions.Read(s);
            Assert.HasCount(2, r);
        }
    }
}
