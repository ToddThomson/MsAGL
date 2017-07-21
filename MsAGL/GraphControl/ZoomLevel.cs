#region Copyright Notice

// Copyright (c) by Achilles Software, All rights reserved.
//
// Licensed under the MIT License. See License.txt in the project root for license information.
//
// Send questions regarding this copyright notice to: mailto:todd.thomson@achilles-software.com

#endregion

#region Namespaces

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#endregion

namespace Msagl.Uwp.UI
{
    public class ZoomLevels
    {
        public static readonly IDictionary<string, double> ZoomPercentages = new Dictionary<string, double>
        {
            { "100 %", 1.0 },
            { "150 %", 1.5 }
        };
    }
}
