using System.ComponentModel;
using Nuke.Common.Tooling;

[TypeConverter(typeof(TypeConverter<Variant>))]
public class Variant : Enumeration
{
    public static readonly Variant Static = new() { Value = "static", IsShared = false };
    public static readonly Variant Shared = new() { Value = "shared", IsShared = true };
    public static readonly Variant Jni = new() { Value = "jni", IsShared = true };
    public static readonly Variant Node = new() { Value = "node", IsShared = true };
    public bool IsShared { get; private set; }
}