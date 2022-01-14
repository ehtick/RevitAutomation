﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using CC_Library;
using CC_Library.Parameters;
using CC_Library.Predictions;
using CC_Plugin.Parameters;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Attributes;
using System.Reflection;

namespace CC_Plugin
{
    internal static class UpdateTab
    {
        private static string dllpath = Assembly.GetExecutingAssembly().Location;
        public const string PName = "AI Update";
        public const string TBName = "Parameter Value";

        public static void CreatePanel(UIControlledApplication uiApp)
        {
            RibbonPanel Panel = uiApp.CreateRibbonPanel(CCRibbon.tabName, PName);
            
            TextBoxData tbd = new TextBoxData(TBName);
            TextBox tb = Panel.AddItem(tbd) as TextBox;
            tb.Width = 350;
            tb.EnterPressed += EnterPressed;

            ComboBoxData cbd = new ComboBoxData("Update Type");
            ComboBox box = Panel.AddItem(cbd) as ComboBox;
            box.AddItem(new ComboBoxMemberData("Masterformat", "Masterformat"));
            box.AddItem(new ComboBoxMemberData("Occupant Load Factor", "Occupant Load Factor"));
            box.AddItem(new ComboBoxMemberData("Brick Pattern", "Brick Pattern"));
            box.AddItem(new ComboBoxMemberData("Herringbone Pattern", "Herringbone Pattern"));
            box.AddItem(new ComboBoxMemberData("Command Training", "Command Training"));
            PushButtonData OLFButtonData = new PushButtonData(
                "Update OLF",
                "Update OLF",
                @dllpath,
                "CC_Plugin.SetOLF");
            PushButton PBOccLoadFactor = Panel.AddItem(OLFButtonData) as PushButton;
        }
        private static void EnterPressed(object sender, TextBoxEnterPressedEventArgs args)
        {
            TextBox textBox = sender as TextBox;
            string text = textBox.Value as string;
            var combotype = args.Application.GetComboData();
            if(combotype == "Predictive")
                combotype = "Masterformat";
            switch(combotype)
            {
                default:
                case "Masterformat":
                    args.Application.SetMasterformat(text);
                    break;
                case "Occupant Load Factor":
                    args.Application.SetOLF(text);
                    break;
                case "Brick Pattern":
                    combotype.CreatePattern(text);
                    break;
                case "Herringbone Pattern":
                    combotype.CreatePattern(text);
                    break;
                case "Command Training":
                    combotype.CreatePattern(text);
                    break;

            }
        }
        private static string GetComboData(this UIApplication app)
        {
            string val = "";
            try
            {
                var panels = app.GetRibbonPanels(CCRibbon.tabName);
                var panel = panels.Where(x => x.Name == PName).First();
                var items = panel.GetItems();
                var item = items.Where(x => x.ItemType == RibbonItemType.ComboBox).First();
                var cb = item as ComboBox;
                val = cb.Current.Name;
            }
            catch (Exception e) { e.OutputError(); }
            return val;
        }
    }
    public static class CMD_SetMasterformat
    {
        public static void SetMasterformat( this UIApplication app, string text )
            //ExternalCommandData commandData,
            //ref string message,
            //ElementSet elements)
        {
            int numb;
            if(int.TryParse(text, out numb))
            {
                Document doc = app.ActiveUIDocument.Document;
                Selection sel = app.ActiveUIDocument.Selection;
                ISelectionFilter selectionFilter = new EleSelectionFilter();

                Reference ChangedObject = sel.PickObject(ObjectType.Element, selectionFilter);
                FamilyInstance inst = doc.GetElement(ChangedObject.ElementId) as FamilyInstance;
                FamilySymbol symb = inst.Symbol;
                NeuralNetwork net = MasterformatNetwork.GetNetwork(CMDLibrary.WriteNull);

                Sample s = new Sample(CC_Library.Datatypes.Datatype.Masterformat);
                s.TextInput = symb.Family.Name;
                s.DesiredOutput = new double[net.Layers.Last().Biases.Count()];
                s.DesiredOutput[numb] = 1;
                MasterformatNetwork.Propogate(s, CMDLibrary.WriteNull);

                using (Transaction t = new Transaction(doc, "Set Param"))
                {
                    t.Start();
                    symb.SetElementParam(Params.Masterformat, numb.ToString());
                    t.Commit();
                }
            }
        }
    }
    public static class CMD_SetOLF
    {
        public static void SetOLF( this UIApplication app, string text )
            //ExternalCommandData commandData,
            //ref string message,
            //ElementSet elements)
        {
            var Factors = Enum.GetNames(typeof(OccLoadFactor)).ToList();
            if(Factors.Any(x => x == "A" + text))
            {
                int numb = Factors.IndexOf("A" + text);
                Document doc = app.ActiveUIDocument.Document;
                Selection sel = app.ActiveUIDocument.Selection;
                ISelectionFilter selectionFilter = new RoomSelectionFilter();

                Reference ChangedObject = sel.PickObject(ObjectType.Element, selectionFilter);
                Room r = doc.GetElement(ChangedObject.ElementId) as Room;
                NeuralNetwork net = OLFNetwork.GetNetwork(CMDLibrary.WriteNull);

                Sample s = new Sample(CC_Library.Datatypes.Datatype.OccupantLoadFactor);
                s.TextInput = r.Name;
                s.DesiredOutput = new double[net.Layers.Last().Biases.Count()];
                s.DesiredOutput[numb] = 1;
                OLFNetwork.Propogate(s, CMDLibrary.WriteNull);

                using (Transaction t = new Transaction(doc, "Set Param"))
                {
                    t.Start();
                    Element e = r as Element;
                    e.Set(Params.OccupantLoadFactor, text);
                    t.Commit();
                }
            }
        }
    }
    public static class CMD_CreatePatterns
    {
        public static void CreatePattern(this string combotype, string text)
        {
            var numbs = text.Split(',');
            double width = double.TryParse(numbs[0], out double a) ? a : 4;
            double height = numbs.Count() >= 2 ? double.TryParse(numbs[1], out double b) ? b : 2 : 2;
            double grout = numbs.Count() >= 3 ? double.TryParse(numbs[2], out double c) ? c : 0 : 0;
            int ratio = numbs.Count() >= 4 ? int.TryParse(numbs[3], out int d) ? d : 2 : 2;
            switch (combotype)
            {
                default:
                case "Brick Pattern":
                    BrickPattern.CreatePattern(width, height, grout, ratio);
                    break;
                case "Herringbone Pattern":
                    HerringbonePattern.CreatePattern(width, height);
                    break;
            }
        }
    }
    public static class CMD_ReadWriteCommandInfo
    {
        public static void WriteCommandInfo(this string combotype, string text)
        {
            var vals[] = text.Split(',');
            Sample s = new Sample(Datatype.Command);
            s.TextInput = vals[0];
            var output = new double[Enum.GetNames(typeof(Command)).Count()];
            output[int.Parse(vals[1])] = 1;
            s.DesiredOutput = output;
            CMDNetwork.Propogate(s, CMDLibrary.WriteNull, true);
        }
    }
    public class EleSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            FamilyInstance inst = element as FamilyInstance;
            if (inst != null)
                return true;
            else
                return false;
        }
        public bool AllowReference(Reference refer, XYZ point) { return true; }
    }
    public class RoomSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            Room r = element as Room;
            if (r != null)
                return true;
            else
                return false;
        }
        public bool AllowReference(Reference refer, XYZ point) { return true; }
    }
}
