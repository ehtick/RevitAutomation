﻿using System;
using System.Linq;
using System.Threading.Tasks;
using CC_Library.Datatypes;

namespace CC_Library.Predictions
{
    public static class MasterformatNetwork3
    {
        private const double dropout = 0.1;
        private const double rate = 0.1;
        public static double[] Predict(string s, WriteToCMDLine write)
        {
            var Alpha = "MasterformatXfmr2".LoadXfmr(CharSet.CharCount * 3, 40, 1000, write);
            var _input = s.Locate(1);
            var AOut = Alpha.Forward(_input);
            var output = AOut.SumRange();
            output = Activations.SoftMax(output);
            return output;
        }
        public static double[] Propogate
            (string[] Samples, WriteToCMDLine write, bool tf = false)
        {
            var results = new double[2];
            var Alpha = "MasterformatXfmr2".LoadXfmr(CharSet.CharCount * 3, 40, 1000, write);
            var AlphaRate = new AttentionChange(Alpha);

            try
            {
                double[] max = new double[Samples.Count()];
                double[] final = new double[Samples.Count()];
                double[] outputs = new double[Samples.Count()];
                double[] desouts = new double[Samples.Count()];
                Parallel.For(0, Samples.Count(), j =>
                {
                    AttentionMem atnmem = new AttentionMem();
                    var _input = Samples[j].Split(',').First().Locate(1);
                    Alpha.Forward(_input, atnmem);
                    var F = Activations.SoftMax(atnmem.attention);

                    max[j] = F[F.ToList().IndexOf(F.Max())];
                    outputs[j] = F.ToList().IndexOf(F.Max());
                    final[j] = F[int.Parse(Samples[j].Split(',').Last())];
                    desouts[j] = int.Parse(Samples[j].Split(',').Last());

                    var DesiredOutput = new double[40];
                    DesiredOutput[int.Parse(Samples[j].Split(',').Last())] = 1;
                    results[0] += CategoricalCrossEntropy.Forward(F, DesiredOutput).Max();
                    results[1] += F.ToList().IndexOf(F.Max()) == int.Parse(Samples[j].Split(',').Last()) ? 1 : 0;

                    var DValues = Activations.InverseCombinedCrossEntropySoftmax(F, DesiredOutput);
                    var dvals = DValues.Dot(atnmem.attn.Ones()); //returns a vector [s.Length, size]
                    Alpha.Backward(atnmem, AlphaRate, dvals);
                });
                final.WriteArray("Desired Output", write);
                max.WriteArray("Max Output", write);
                outputs.WriteArray("Outputs", write);
                desouts.WriteArray("Desired", write);
            }
            catch (Exception e) { e.OutputError(); }
            //MFMem.Update(Samples.Count(), rate, net);
            Alpha.Update(AlphaRate, write);
            results[0] /= Samples.Count();
            results[1] /= Samples.Count();

            write("Run Error : " + results[0]);
            write("Run Accuracy : " + results[1]);
            //net.Save();
            string Folder = "NeuralNets".GetMyDocs();
            Alpha.Save(Folder);
            return results;
        }
    }
}
