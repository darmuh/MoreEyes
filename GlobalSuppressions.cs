// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0031:Use null propagation", Justification = "can't use null coalescing operator as Unity Objects do not return null with this operator when they are destroyed - https://discussions.unity.com/t/c-null-coalescing-operator-does-not-work-for-unityengine-object-types/710219", Scope = "member", Target = "~M:MoreEyes.EyeManagement.EyeRef.SetColorIris(UnityEngine.Color)")]
[assembly: SuppressMessage("Style", "IDE0031:Use null propagation", Justification = "can't use null coalescing operator as Unity Objects do not return null with this operator when they are destroyed - https://discussions.unity.com/t/c-null-coalescing-operator-does-not-work-for-unityengine-object-types/710219", Scope = "member", Target = "~M:MoreEyes.EyeManagement.EyeRef.SetColorPupil(UnityEngine.Color)")]
