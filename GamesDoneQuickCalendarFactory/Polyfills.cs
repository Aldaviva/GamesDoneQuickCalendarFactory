using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable All
#pragma warning disable IDE0130

namespace System.Runtime.CompilerServices {

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ExcludeFromCodeCoverage]
    internal sealed class UnionAttribute: Attribute;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IUnion {

        public object? Value { get; }

    }

}