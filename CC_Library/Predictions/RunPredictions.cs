using System;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using CC_Library.Datatypes;

namespace CC_Library.Predictions
{
    public static class Datasets
    {
        public static void RunPredictions(WriteToCMDLine write)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                FileName = "Select a text file",
                Title = "Open txt file"
            };
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                int runs = 0;
                double er = 0;
                double acc = 0;
                var filepath = ofd.FileName;
                var dir = Path.GetDirectoryName(filepath);
                var Files = Directory.GetFiles(dir);
                Random random = new Random();
                for(int i = 0; i < 10000; i++)
                {
                    string f = Files[random.Next(Files.Count())];
                    try
                    {
                        var error = CutLineWeightNetwork.Propogate(f, write);
                        /*
                        Sample s = f.ReadFromBinaryFile<Sample>();
                        string datatype = s.Datatype;
                        var error = MasterformatNetwork.Propogate(s, write, true);
                        */
                        if (error[0] > 0)
                        {
                            runs++;
                            er += error[0];
                            acc += error[1];

                            write("Total Runs : " + runs);
                            write("Total Error : " + er / runs);
                            write("Total Accuracy : " + acc / runs);
                            write("");
                        }
                        else
                        {
                            write("");
                            write("error was 0");
                            write("");
                        }
                    }
                    catch (Exception e) { e.OutputError(); }
                    System.Threading.Thread.Sleep(500);

                }
            }
        }
    }
}
