﻿using PowerVBA.Core.Wrap.WrapBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerVBA.Core.Connector;
using Microsoft.Vbe.Interop;

namespace PowerVBA.V2010.WrapClass
{
    public class ReferenceWrapping : ReferenceWrappingBase
    {
        public override PPTVersion ClassVersion => PPTVersion.PPT2010;

        public ReferenceWrapping(Reference reference)
        {
            this.reference = reference;
        }

        public Reference reference { get; }
        
        public References Collection => reference.Collection;
        public VBE VBE => reference.VBE;
        public override string Name => reference.Name;
        public string Guid => reference.Guid;
        public int Major => reference.Major;
        public int Minor => reference.Minor;
        public override string FullPath => reference.FullPath;
        public bool BuiltIn => reference.BuiltIn;
        public bool IsBroken => reference.IsBroken;
        public vbext_RefKind Type => reference.Type;
        public string Description => reference.Description;
    }
}
