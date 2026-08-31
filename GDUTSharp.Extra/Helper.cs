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
}
