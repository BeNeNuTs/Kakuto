using System;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class ConditionalFieldAttribute : PropertyAttribute
{
    public readonly string[] PropertiesToCheck;
    public readonly object CompareValue;

    public ConditionalFieldAttribute(object compareValue, params string[] propertiesToCheck)
    {
        PropertiesToCheck = propertiesToCheck;
        CompareValue = compareValue;
    }
}