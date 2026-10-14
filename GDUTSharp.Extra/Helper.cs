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
        IEnumerable<WSDescOpt> descriptions,
        WSHeaderOpt header,
        WSContentOpt<T> content,
        bool isAdjustToContents,
        bool isFreezeHeaderAndDesc,
        string? fontName = null)
    {
        int descriptionRowCount = 0, contentRowCount = 0, headerCount = header.Headers.Count();
        foreach (var item in descriptions)
        {
            descriptionRowCount++;
            var c = worksheet.Cell(descriptionRowCount, 1);
            c.Value = item.Content;
            if (item.Hyperlink is not null)
            {
                c.SetHyperlink(new(item.Hyperlink));
            }
            worksheet.Range(descriptionRowCount, 1, descriptionRowCount, headerCount).Merge();
        }
        if (fontName is not null && descriptionRowCount > 0)
        {
            worksheet.Range(1, 1, descriptionRowCount, 1).Style.Font.FontName = fontName;
        }
        var headerRange = worksheet.Cell(descriptionRowCount + 1, 1).InsertData(header.Headers, true);
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRange.Style.Fill.BackgroundColor = header.HeaderColor;
        if (fontName is not null)
        {
            headerRange.Style.Font.FontName = fontName;
        }
        if (isFreezeHeaderAndDesc)
        {
            worksheet.SheetView.FreezeRows(descriptionRowCount + 1);
        }
        if (header.IsAutoFilter)
        {
            headerRange.SetAutoFilter();
        }
        foreach (var item in content.Source)
        {
            contentRowCount++;
            int row = descriptionRowCount + 1 + contentRowCount;
            var r = worksheet.Cell(row, 1).InsertData(content.GetContent(item), true);
            if (fontName is not null)
            {
                r.Style.Font.FontName = fontName;
            }
        }
        if (isAdjustToContents)
        {
            worksheet.Columns(1, headerCount).AdjustToContents();
        }
    }

    public record class WSDescOpt(string Content, string? Hyperlink = null);
    public record class WSHeaderOpt(IEnumerable<string> Headers, XLColor HeaderColor, bool IsAutoFilter);
    public record class WSContentOpt<T>(Func<T, IEnumerable<object>> GetContent, IEnumerable<T> Source);

    public static readonly List<WSDescOpt> WSDesc =
        [
            new("Powered by GDUTSharp", GSConst.GDUTSharpRepo),
            new("数据通过 GDUTSharp.Extra 从教学服务系统导出，一切以教学服务系统为准"),
        ];

    #endregion
}
