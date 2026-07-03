using System.Runtime.InteropServices;

namespace FFF.Net;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct NativeBool
{
    private readonly byte _value;
    private NativeBool(byte value) => _value = value;
    public static readonly NativeBool False = new(0);
    public static readonly NativeBool True = new(1);

    public static implicit operator bool(NativeBool val) => val._value != 0;
    public static implicit operator NativeBool(bool val) => val ? True : False;
    public static implicit operator byte(NativeBool val) => val._value;
    public static implicit operator NativeBool(byte val) => new(val);
    public override string ToString() => ((bool)this).ToString();
}