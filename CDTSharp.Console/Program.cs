//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
﻿using CDTSharp.Core;

namespace CDTSharp.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var reader = CDTReader.ReadCDTFile(args[0]);
        }
    }
}
