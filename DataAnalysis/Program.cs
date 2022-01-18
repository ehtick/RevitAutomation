﻿using System;
using CC_Library;
using CC_Library.Predictions;
using CC_Library.Datatypes;
using System.IO;
using System.Diagnostics;

namespace DataAnalysis
{
    class Program
    {
        public static string Write(string wo)
        {
            Console.WriteLine(wo);
            return wo;
        }
        [STAThread]
        static void Main(string[] args)
        {
            bool x = true;
            while(x)
            {
                Console.WriteLine("Enter the command to run");
                Console.WriteLine("Commands are : Train, Sort, Test, Show, Revise");
                string line = Console.ReadLine();
                line = line.ToUpper();
                try
                {
                    switch (line)
                    {
                        default:
                            break;
                        case "TRAIN":
                            bool c = true;
                            while(c)
                            {
                                Console.WriteLine("Enter a Datatype to Predict");
                                string dt = Console.ReadLine();
                                try
                                {
                                    Datatype datatype = Enum.Parse(dt, typeof(Datatype));
                                    switch(datatype)
                                    {
                                        default:
                                        case Datatype.Masterformat:
                                            break;
                                        case Datatype.Command:
                                            for(int i = 0; i < 1000; i++)
                                            {
                                                CmdNetwork.Propogate(Write);
                                            }
                                            break;
                                    }
                                    Console.WriteLine("Continue? Y / N");
                                    string cont = Console.ReadLine().ToUpper();
                                    if(cont == "N")
                                        c = false;
                                }
                                catch
                                {
                                }
                            }
                            Datasets.RunPredictions(new WriteToCMDLine(Write));
                            break;
                        case "SORT":
                            Datatype.Masterformat.Sort();
                            break;
                        case "TEST":
                            Console.WriteLine("Enter a Datatype to Predict");
                            string l2 = Console.ReadLine();
                            bool c = true;
                            while (c)
                            {
                                Console.WriteLine("Enter a phrase to predict.");
                                string l3 = Console.ReadLine();
                                PredTest.TestPredictions(l3, l2, new WriteToCMDLine(Write));
                                Console.WriteLine("Continue? Y / N");
                                string l4 = Console.ReadLine().ToUpper();
                                if (l4 == "N")
                                    c = false;
                                Console.WriteLine("");
                            }
                            break;
                        case "SHOW":
                            string s = "";
                            while(s != "X")
                            {
                                s = Console.ReadLine();
                                s.TestChars(new WriteToCMDLine(Write));
                            }
                            break;
                        case "READ":
                            ReadSample.Read(new WriteToCMDLine(Write));
                            break;
                        case "BUYSELL":
                        case "BS":
                            while (true)
                            {
                                TaskScheduler.Instance.ScheduleCycle(.006);

                                string l1 = Console.ReadLine();
                                if (l1 == "x")
                                    break;
                            }
                            break;
                        case "EXIT":
                        case "E":
                        case "X":
                            x = false;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    WriteError(ex);
                }
            }
        }
        static void WriteError(Exception ex)
        {
            string filePath = "Error.txt";
            string fullpath = filePath.GetMyDocs();

            using (StreamWriter writer = new StreamWriter(fullpath, true))
            {
                writer.WriteLine("-----------------------------------------------------------------------------");
                writer.WriteLine("Date : " + DateTime.Now.ToString());
                writer.WriteLine();

                while (ex != null)
                {
                    writer.WriteLine(ex.GetType().FullName);
                    writer.WriteLine("Message : " + ex.Message);
                    writer.WriteLine("StackTrace : " + ex.StackTrace);

                    ex = ex.InnerException;
                }
            }
        }
    }
}
