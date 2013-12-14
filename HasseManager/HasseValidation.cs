/*
HasseManager - a program for construction and mining of Hasse diagrams.
Copyright (C) 2012  Peter Lind

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA. 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HasseManager
{
    class HasseValidation
    {
        public static void CheckCollectionsHaveSameObjects(HasseNodeCollection correctLub, HasseNodeCollection c2)
        {
            foreach (HasseNode elm1 in correctLub.Values )
            {
                if (!c2.ContainsKey(elm1.KeyString ))
                    throw new Exception("CheckCompareCollections: missing element: " + elm1.KeyString);
            }
            foreach (HasseNode elm2 in c2.Values )
            {
                if (!correctLub.ContainsKey(elm2.KeyString ))
                    throw new Exception("CheckCompareCollections: extra element" + elm2.KeyString);
            }
        }

    }
}
