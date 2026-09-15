namespace GDUTSharp.Shared.Attributes
{
    /// <summary>
    /// 通过源生成器自动覆写 <see cref="object.ToString"/>
    /// </summary>
    /// <remarks>
    /// 覆写为以下形式：
    /// <code>
    /// objectName
    ///   - Prop0:{Prop0.Value}
    ///   - Prop1:{Prop1.Value}
    ///   - Prop2:{Prop2.Value}
    /// </code>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class OverrideToString(string? displayName = null) : Attribute
    {
        // TODO: 根据 DisplayName 修改显示的类名
        public string? DisplayName { get; set; } = displayName;
    }
}
