﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodest.Revit.UI;

//note: zero point will work in a 1/8" bounding rectangle that serves as a border between details.
//standard Width for a 1x detail is 7 1/4" + 1/8" border Width = 7 3/8" Wide
//standard Height for a detail is 6 3/4" High with a 1/2" gap. between. The bottom includes an additional 1/8" from bottom. The top includes 1/2"
//(not sure why the additional is at the top.)
//Space for a title is 3/4" w/ 1/8" gap above.


namespace CC_ZeroPoint
{
    public class GetZeroPointBox
    {
        private const double Width = 7.375 / 12;
        private const double Height = 7.25 / 12;
        private const double Title = 1.25 / 12;
        private const double Notes = 2.125 / 12;
        
        public static void CreateBox(UIControlledApplication app)
        {
            var doc = app.ActiveUIDocument.Document;
            var view = doc.ActiveView;
            
            if(view.ViewType == ViewType.DraftingView)
            {
                var scale = view.Scale;
                XYZ p1 = new XYZ(0,0,0);
                XYZ p2 = new XYZ(Width * Scale, 0, 0);
                XYZ p3 = new XYZ(0, Height * Scale, 0);
                XYZ p4 = new XYZ(Width * Scale, Height * Scale, 0);
                
                var cut = new XYZ(0, 0, 1);
                
                using(Transaction t = new Transaction(doc, "Create Zeros"))
                {
                    t.Start();
                    
                    var rp1 = doc.Create.NewReferencePlane(p1, p2, cut, view);
                    var rp2 = doc.Create.NewReferencePlane(p1, p3, cut, view);
                    var rp3 = doc.Create.NewReferencePlane(p3, p4, cut, view);
                    var rp4 = doc.Create.NewReferencePlane(p2, p4, cut. view);
                    
                    t.Commit();
                }
            }
            else { TaskDialog.Show("Error", "Activate a Drafting View Before Use"); }
        }
    }
}
