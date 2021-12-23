﻿using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Autodesk.Revit.DB;
using System.Windows.Forms;
using CC_Library.Datatypes;

namespace CC_Plugin
{
    //Reframe the hatch from 0 to 1, include a comment that tells the user what to scale it to!!!!
    public class HatchEditor
    {
        public static void EditHatch(Document doc)
        {
            var v = doc.ActiveView;
            var lines = new FilteredElementCollector(doc, v.Id).OfCategory(BuiltInCategory.OST_Lines).ToElementIds().ToList();
            List<double[]> points = new List<double[]>();
            for (int i = 0; i < lines.Count(); i++)
            {
                var line = doc.GetElement(lines[i]) as DetailLine;
                if (line != null)
                {
                    var pt = new double[4];
                    pt[0] = Math.Round(line.GeometryCurve.GetEndPoint(0).X, 6);
                    pt[1] = Math.Round(line.GeometryCurve.GetEndPoint(0).Y, 6);
                    pt[2] = Math.Round(line.GeometryCurve.GetEndPoint(1).X, 6);
                    pt[3] = Math.Round(line.GeometryCurve.GetEndPoint(1).Y, 6);
                    points.Add(pt);
                }
            }
            var ext = GetExtents(points);
            var text = new List<string>();
            text.Add("*Title");
            text.Add(";%TYPE=MODEL,");
            foreach (var pt in points)
                text.Add(GetText(pt, ext));
            SaveFileDialog sfd = new SaveFileDialog()
            {
                FileName = "Create a pattern file",
                Filter = "PAT files (*.pat)|*.pat",
                Title = "Create a pat file"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var fp = sfd.FileName;
                if (fp.EndsWith(".txt"))
                    fp.Replace(".txt", ".pat");
                if(!fp.EndsWith(".pat"))
                    fp += ".pat";
                text[0] = "*" + fp.Split('\\').Last().Split('.').First();
                File.WriteAllLines(fp, text);
            }
        }
        private static double[] GetExtents(List<double[]> Points)
        {
            double[] extents = new double[4] { Points[0][0], Points[0][1], Points[0][2], Points[0][3] };
            for(int i = 1; i < Points.Count(); i++)
            {
                extents[0] = Math.Min(extents[0], Math.Min(Points[i][0], Points[i][2]));
                extents[1] = Math.Min(extents[1], Math.Min(Points[i][1], Points[i][3]));
                extents[2] = Math.Max(extents[2], Math.Max(Points[i][0], Points[i][2]));
                extents[3] = Math.Max(extents[3], Math.Max(Points[i][1], Points[i][3]));
            }
            return extents;
        }
        private static string GetText(double[] point, double[] extents)
        {
            var pt = Reframe(point, extents);
            var dir = GetAngle(pt);
            var origin = GetOrigin(pt);
            var shift = GetShift(pt);
            var pendown = Length(pt);
            var penup = -1 * RepLength(pt, extents);

            return
                dir + ", " + Math.Round(origin[0], 6) + ", " + Math.Round(origin[1], 6) + ", " +
                Math.Round(shift[0], 6) + ", " + Math.Round(shift[1], 6) + ", " + Math.Round(pendown, 6) + ", " + Math.Round(penup, 6);
        }
        private static double[] Reframe(double[] point, double[] extents)
        {
            var maxx = extents[2] - extents[0];
            var maxy = extents[3] - extents[1];
            var max = Math.Max(maxx, maxy);

            var minx = extents[0];
            var miny = extents[1];

            var ang = GetAngle(point);
            if(ang > 90 || ang < -90)
            {
                return new double[4]
                {
                    (point[2] - minx) / max,
                    (point[3] - miny) / max,
                    (point[0] - minx) / max,
                    (point[1] - miny) / max
                };
            }
            return new double[4]
            {
                (point[0] - minx) / max,
                (point[1] - miny) / max,
                (point[2] - minx) / max,
                (point[3] - miny) / max
            };
        }
        private static double GetAngle(double[] line)
        {
            var angle = 180 * Math.Atan2(line[3] - line[1], line[2] - line[0]) / Math.PI;
            angle = Math.Round(angle, 3);
            return angle;
        }
        private static double[] GetOrigin(double[] line)
        {
            return new double[2] { line[0], line[1] };
            /*
            var dir = -1 * Math.Atan2(line[3] - line[1], line[2] - line[0]);
            var rotx = (line[0] * Math.Cos(dir)) - (line[1] * Math.Sin(dir));
            var roty = (line[1] * Math.Cos(dir)) + (line[0] * Math.Sin(dir));
            return new double[2] { rotx, roty };
            */
        }
        private static double[] GetShift(double[] line)
        {
            var dir = GetAngle(line);
            var X = Math.Sin(dir * Math.PI / 180);
            var Y = Math.Cos(dir * Math.PI / 180);
            return new double[2] { X, Y };
        }
        private static double RepLength(double[] line, double[] extents)
        {
            var ang = GetAngle(line);
            if (ang == 0 || ang == 90 || ang == -90)
                return 1 - Length(line);

            //distance across the length of the pattern that the line is
            var yprime = Math.Tan(ang * Math.PI / 180);
            yprime = Math.Round(yprime, 6);

            var gcom = gcd(1 * 1e6, yprime * 1e6);
            var 
            var xoffset = 1 / yprime;
            var dist = (Math.Sin(ang * Math.PI / 180)) / xoffset;
            return Length(line) - dist;
        }
        private static double gcd(double a, double b)
        {
            if (b == 0)
                return a;
            return (gcd(b, a % b));
        }
        private static double Length(double[] point)
        {
            var x = (point[2] - point[0]) * (point[2] - point[0]);
            var y = (point[3] - point[1]) * (point[3] - point[1]);
            return Math.Sqrt(x + y);
        }
        private static double[] Intersection(double[] p1, double[] p2)
        {
            var a = p1[0] * p1[3];
            var b = p1[1] * p1[2];
            var c = p2[0] - p2[2];
            var d = p1[0] - p1[2];
            var e = p2[0] * p2[3];
            var f = p2[1] * p2[2];
            var g = p1[0] - p1[2];
            var h = p2[1] - p2[3];
            var i = p1[1] - p1[3];
            var j = p2[0] - p2[2];
            var denom = (g * h) - (i * j);
            var enumx = ((a - b) * c) - (d * (e - f));
            var enumy = ((a - b) * h) - (i * (e - f));
            var x = enumx / denom;
            var y = enumy / denom;
            return new double[2] {x, y};
        }
    }
}
