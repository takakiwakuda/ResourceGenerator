// URL: https://github.com/takakiwakuda/CSharp-Polyfill

#if !NET5_0_OR_GREATER
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Diagnostics.CodeAnalysis
{
#if !NETCOREAPP3_0_OR_GREATER
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
    [DebuggerNonUserCode]
    [ExcludeFromCodeCoverage]
    internal sealed class AllowNullAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
    [DebuggerNonUserCode]
    [ExcludeFromCodeCoverage]
    internal sealed class DisallowNullAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
    [DebuggerNonUserCode]
    [ExcludeFromCodeCoverage]
    internal sealed class MaybeNullAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
    [DebuggerNonUserCode]
    [ExcludeFromCodeCoverage]
    internal sealed class NotNullAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    [DebuggerNonUserCode]
    [ExcludeFromCodeCoverage]
    [SuppressMessage("csharp", "IDE0060")]
    internal sealed class MaybeNullWhenAttribute : Attribute
    {
        internal MaybeNullWhenAttribute(bool returnValue)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    [DebuggerNonUserCode]
    [ExcludeFromCodeCoverage]
    [SuppressMessage("csharp", "IDE0060")]
    internal sealed class NotNullWhenAttribute : Attribute
    {
        internal NotNullWhenAttribute(bool returnValue)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
    [DebuggerNonUserCode]
    [ExcludeFromCodeCoverage]
    [SuppressMessage("csharp", "IDE0060")]
    internal sealed class NotNullIfNotNullAttribute : Attribute
    {
        internal NotNullIfNotNullAttribute(string parameterName)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    [DebuggerNonUserCode]
    [ExcludeFromCodeCoverage]
    [SuppressMessage("csharp", "IDE0060")]
    internal sealed class DoesNotReturnAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    [DebuggerNonUserCode]
    [ExcludeFromCodeCoverage]
    [SuppressMessage("csharp", "IDE0060")]
    internal sealed class DoesNotReturnIfAttribute : Attribute
    {
        internal DoesNotReturnIfAttribute(bool parameterValue)
        {
        }
    }
#endif

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    [DebuggerNonUserCode]
    [ExcludeFromCodeCoverage]
    [SuppressMessage("csharp", "IDE0060")]
    internal sealed class MemberNotNullAttribute : Attribute
    {
        internal MemberNotNullAttribute(string member)
        {
        }

        internal MemberNotNullAttribute(params string[] members)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    [DebuggerNonUserCode]
    [ExcludeFromCodeCoverage]
    [SuppressMessage("csharp", "IDE0060")]
    internal sealed class MemberNotNullWhenAttribute : Attribute
    {
        internal MemberNotNullWhenAttribute(bool returnValue, string member)
        {
        }

        internal MemberNotNullWhenAttribute(bool returnValue, params string[] members)
        {
        }
    }
}
#endif
