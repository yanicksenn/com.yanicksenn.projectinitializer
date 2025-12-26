using System;
using System.Collections.Generic;
using UnityEngine;

namespace YanickSenn.ProjectInitializer.Editor.Anchors
{
    public abstract class VariableAnchor<TValue> : AbstractAnchor where TValue : IComparable<TValue> {
    }
}
