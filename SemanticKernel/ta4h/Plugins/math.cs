﻿// Copyright (c) Microsoft. All rights reserved.

using System.ComponentModel;
using System.Globalization;
using Microsoft.SemanticKernel.SkillDefinition;


public class Math
{
    [SKFunction, Description("Take the square root of a number")]
    public string Sqrt(string number)
    {
        return System.Math.Sqrt(Convert.ToDouble(number, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture);
    }
}