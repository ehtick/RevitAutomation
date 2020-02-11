﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System;

namespace CC_Library
{
    public class AdjustPredictions
    {
        private static string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static string InputFile = directory + "\\CC_MFData.xml";
        private static string OutputFile = directory + "\\CC_MasterformatPredictor.xml";

        public static List<Prediction> RunFormula(List<PredictionElement> PEs)
        {
            List<Prediction> data = new List<Prediction>();
            foreach(var p in PEs)
            {
                foreach(var d in p.Options)
                {
                }
            }
        }
        public static void run()
        {
            if(File.Exists(InputFile))
            {
                XDocument indoc = XDocument.Load(InputFile);
                double MaxChange = (1 / (Math.Pow(PredictionNumber, 2) + 1));
                double mv = Data.Max();
                int Guess = Array.IndexOf(Data, mv);
                if(Guess == Correct && mv > 0.75)
                {
                    Predictions[Correct] += MaxChange / 2;
                }
                else
                {
                    Predictions[Correct] += MaxChange;
                    for(int i = 0; i < Predictions.Count(); i++)
                    {
                        if(i != Correct)
                           Predictions[i] -= (MaxChange / PredictionCount);
                    }
                }
                PredictionNumber += 1;
                }
            }
        }
    }
}
