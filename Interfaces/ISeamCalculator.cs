﻿using SeamCarving.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeamCarving.Interfaces
{
    interface ISeamCalculator
    {
        void calculateSeam(SeamCarvingContext injectedContext);
    }
}
