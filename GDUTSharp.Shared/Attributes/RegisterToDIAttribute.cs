namespace GDUTSharp.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public class RegisterToDIAttribute : Attribute
{
    public RegisterToDIAttribute(string interfaceName)
    {
        _ = interfaceName;
    }
}
