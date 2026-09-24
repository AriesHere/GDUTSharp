using ClosedXML.Excel;
using GDUTSharp.Shared;

namespace GDUTSharp.Extra;

public static class Helper
{
    /// <param name="sortedList">请确保升序</param>
    public static List<List<int>> SplitIntoConsecutiveGroups(this List<int> sortedList)
    {
        var groups = new List<List<int>>();
        List<int> currentGroup = [sortedList[0]];
        for (int i = 1; i < sortedList.Count; i++)
        {
            if (sortedList[i] - sortedList[i - 1] == 1)
            {
                currentGroup.Add(sortedList[i]);
            }
            else
            {
                groups.Add(currentGroup);
                currentGroup = [sortedList[i]];
            }
        }
        groups.Add(currentGroup);
        return groups;
    }

    #region excel

    public static void FillDataToWrokSheet<T>(
        this IXLWorksheet worksheet,
        IList<WSDescOpt> description,
        WSHeaderOpt header,
        WSContentOpt<T> content,
        bool isAdjustToContents,
        bool isFreezeHeaderAndDesc,
        string? fontName = null)
    {
        for (int i = 0; i < description.Count; i++)
        {
            var c = worksheet.Cell(i + 1, 1);
            c.Value = description[i].Content;
            if (description[i].Hyperlink is not null)
            {
                c.SetHyperlink(new(description[i].Hyperlink));
            }
            if (fontName is not null)
            {
                worksheet.Range(1, 1, description.Count, 1).Style.Font.FontName = fontName;
            }
            worksheet.Range(i + 1, 1, i + 1, header.Headers.Count).Merge();
        }
        var headerRange = worksheet.Cell(description.Count + 1, 1).InsertData(header.Headers, true);
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRange.Style.Fill.BackgroundColor = header.HeaderColor;
        if (fontName is not null)
        {
            headerRange.Style.Font.FontName = fontName;
        }
        if (isFreezeHeaderAndDesc)
        {
            worksheet.SheetView.FreezeRows(description.Count + 1);
        }
        if (header.IsAutoFilter)
        {
            headerRange.SetAutoFilter();
        }
        for (int i = 0; i < content.Source.Count; i++)
        {
            var r = worksheet.Cell(description.Count + 1 + i + 1, 1).InsertData(content.GetContent(content.Source[i]), true);
            if (fontName is not null)
            {
                r.Style.Font.FontName = fontName;
            }
        }
        if (isAdjustToContents)
        {
            worksheet.Columns(1, header.Headers.Count).AdjustToContents();
        }
    }

    public record class WSDescOpt(string Content, string? Hyperlink = null);
    public record class WSHeaderOpt(IList<string> Headers, XLColor HeaderColor, bool IsAutoFilter);
    public record class WSContentOpt<T>(Func<T, IList<object>> GetContent, IList<T> Source);

    public static readonly List<WSDescOpt> WSDesc =
        [
            new("Powered by GDUTSharp", GSConst.GDUTSHARP_REPO),
            new("数据通过 GDUTSharp.Extra 从教学服务系统导出，一切以教学服务系统为准"),
        ];

    #endregion
}
