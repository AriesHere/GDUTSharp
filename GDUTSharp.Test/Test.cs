using System.Diagnostics;
using System.Security.Cryptography;
using GDUTSharp.Extra;
using GDUTSharp.Services;
using GDUTSharp.Shared;
using GDUTSharp.Test.TestServers;

namespace GDUTSharp.Test
{
    [TestClass]
    public sealed class Test
    {
        [TestMethod]
        [DataRow("""
            "课程名称","班级名称","人数","教师","周次","星期","节次","上课地点","排课日期","课序","类型","授课内容简介",
            "劳动教育","某某班级(1),某某班级(2)","60","某某老师","1","4","08","某上课地点","2026-09-07","1","实验教学","测试简介"
            "大学美育(1)","某某班级(1),某某班级(2),某某班级(3),某某班级(4)","98","某某老师","1","1","101112","某上课地点","2026-09-07","1","理论教学","简介内容"
            """)]
        [DataRow("""
            <html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:x='urn:schemas-microsoft-com:office:excel' xmlns='http://www.w3.org/TR/REC-html40'><head><style type='text/css'>td { mso-number-format: '@'; }</style><meta http-equiv="content-type" content="text/html; charset=UTF-8" /></head><body><table style='mso-number-format:\@;'><tr><th rowspan='1' colspan='1' field='kcmc' style='background-color:#D3D3D3;font-weight:bold;'>课程名称</th><th rowspan='1' colspan='1' field='jxbmc' style='background-color:#D3D3D3;font-weight:bold;'>班级名称</th><th rowspan='1' colspan='1' field='pkrs' style='background-color:#D3D3D3;font-weight:bold;'>人数</th><th rowspan='1' colspan='1' field='teaxms' style='background-color:#D3D3D3;font-weight:bold;'>教师</th><th rowspan='1' colspan='1' field='zc' style='background-color:#D3D3D3;font-weight:bold;'>周次</th><th rowspan='1' colspan='1' field='xq' style='background-color:#D3D3D3;font-weight:bold;'>星期</th><th rowspan='1' colspan='1' field='jcdm' style='background-color:#D3D3D3;font-weight:bold;'>节次</th><th rowspan='1' colspan='1' field='jxcdmc' style='background-color:#D3D3D3;font-weight:bold;'>上课地点</th><th rowspan='1' colspan='1' field='pkrq' style='background-color:#D3D3D3;font-weight:bold;'>排课日期</th><th rowspan='1' colspan='1' field='kxh' style='background-color:#D3D3D3;font-weight:bold;'>课序</th><th rowspan='1' colspan='1' field='jxhjmc' style='background-color:#D3D3D3;font-weight:bold;'>类型</th><th rowspan='1' colspan='1' field='sknrjj' style='background-color:#D3D3D3;font-weight:bold;'>授课内容简介</th></tr><tr><td>劳动教育</td><td>某某班级(1),某某班级(2)</td><td>60</td><td>某某老师</td><td>1</td><td>4</td><td x:str="08">08</td><td>某上课地点</td><td>2026-09-07</td><td>1</td><td>实验教学</td><td>测试简介</td></tr><tr><td>大学美育(1)</td><td>某某班级(1),某某班级(2),某某班级(3),某某班级(4)</td><td>98</td><td>某某老师</td><td>1</td><td>1</td><td x:str="101112">101112</td><td>某上课地点</td><td>2026-09-07</td><td>1</td><td>理论教学</td><td>简介内容</td></tr></table></body></html>
            """)]
        public void TestJXFWExportedLessonsFileAnalysis(string input)
        {
            MemoryStream s = new(input.ToBytes());
            var r = ExtraExtensions.Read(s, ExtraExtensions.JXFWFileType.AutoDetect);
            Assert.HasCount(2, r);
        }

        [TestMethod]
        [DataRow("2026春季", 202502)]
        [DataRow("2026秋季", 202601)]
        public void TermStringToInt6Digit(string raw, int expected)
        {
            var r = Shared.Helper.TermStringToInt6Digit(raw);
            Assert.AreEqual(expected, r);
        }

        [TestMethod]
        public void TestEncrypt()
        {
            SecurityService s = new(new TestLogger<SecurityService>());
            byte[] raw = "System.Security.Cryptography.CryptographicAException: Specified key is not a valid size for this algorithm.".ToBytes();
            var key = RandomNumberGenerator.GetBytes(32);
            var iv = s.GenIV();
            var cipherText = s.AesCbcEncrypt(raw, key, iv);
            var result = s.AesCbcDecrypt(cipherText, key, iv);
            Assert.AreEqual(raw.GetString(), result.GetString());
        }
    }
}
