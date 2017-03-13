﻿using Microsoft.Office.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerVBA.Core.Extension
{
    public static class BoolEx
    {
        public static MsoTriState BoolToState(this bool Data)
        {
            if (Data) return MsoTriState.msoTrue;
            else return MsoTriState.msoFalse;
        }
    }
}
