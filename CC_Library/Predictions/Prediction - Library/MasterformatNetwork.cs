﻿using System;
using System.Linq;
using System.Threading.Tasks;
using CC_Library.Datatypes;

namespace CC_Library.Predictions
{
    public static class MasterformatNetwork
    {
        private const double dropout = 0.1;
        private const double rate = 1e-4;
        public static Datatype datatype { get { return Datatype.Masterformat; } }
        public static NeuralNetwork GetNetwork(WriteToCMDLine write)
        {
            //Alpha2 a = new Alpha2(CMDLibrary.WriteNull);
            NeuralNetwork net = datatype.LoadNetwork(write);
            if (net.Datatype == Datatype.None)
            {
                net = new NeuralNetwork(datatype);
                net.Layers.Add(new Layer(300, AlphaAttn.size, Activation.LRelu, 1e-5, 1e-5));
                net.Layers.Add(new Layer(300, net.Layers.Last(), Activation.LRelu, 1e-5, 1e-5));
                // net.Layers.Add(new Layer(100, net.Layers.Last(), Activation.LRelu, 1e-5, 1e-5));
                //net.Layers.Add(new Layer(100, net.Layers.Last(), Activation.LRelu, 1e-5, 1e-5));
                //net.Layers.Add(new Layer(100, net.Layers.Last(), Activation.LRelu, 1e-5, 1e-5));
                net.Layers.Add(new Layer(40, net.Layers.Last(), Activation.CombinedCrossEntropySoftmax));
            }
            return net;
        }
        public static double[] Predict(string s, WriteToCMDLine write)
        {
            return null;
            /*
            NeuralNetwork net = GetNetwork(write);
            //Alpha2 a = new Alpha2(write);
            //a.Load(write);
            //double[] Results = a.Forward(s, write).Key;
            //Results.WriteArray("Alpha Results : ", write);
            for(int i = 0; i < net.Layers.Count(); i++)
            {
                Results = net.Layers[i].Output(Results);
            }
            return Results;
            */
        }
        public static double[] Propogate
            (string[] Samples, WriteToCMDLine write, bool tf = false)
        {
            var results = new double[2];
            NeuralNetwork net = GetNetwork(write);
            var Alpha = AlphaAttn.Load(write);
            var AlphaRate = new AttentionChange();
            //Alpha2 a = new Alpha2(write);
            //a.Load(write);
            //var am = a.CreateMemory();
            NetworkMem MFMem = new NetworkMem(net);

            try
            {
                double[] alpharesults = new double[Samples.Count()];
                double[] outputs = new double[Samples.Count()];
                double[] desouts = new double[Samples.Count()];
                Parallel.For(0, Samples.Count(), j =>
                {
                    AttentionMem atnmem = new AttentionMem();
                    //var output = a.Forward(Samples[j].Split(',').First(), write);
                    Alpha.Forward(Samples[j].Split(',').First(), atnmem);
                    var F = net.Forward(atnmem.attention, dropout, write, false);

                    alpharesults[j] = atnmem.attention.Sum();
                    outputs[j] = F.Last()[0].ToList().IndexOf(F.Last()[0].Max());
                    desouts[j] = int.Parse(Samples[j].Split(',').Last());

                    var DesiredOutput = new double[40];
                    DesiredOutput[int.Parse(Samples[j].Split(',').Last())] = 1;
                    results[0] += CategoricalCrossEntropy.Forward(F.Last()[0], DesiredOutput).Max();
                    results[1] += F.Last()[0].ToList().IndexOf(F.Last()[0].Max()) == int.Parse(Samples[j].Split(',').Last()) ? 1 : 0;

                    var DValues = net.Backward(F, DesiredOutput, MFMem, write);
                    //a.Backward(DValues, output.Value, am, write, j == 0);
                    Alpha.Backward(atnmem, AlphaRate, DValues);
                });
                alpharesults.WriteArray("Alpha Results", write);
                outputs.WriteArray("Outputs", write);
                desouts.WriteArray("Desired", write);
            }
            catch (Exception e) { e.OutputError(); }
            //MFMem.Update(Samples.Count(), rate, net);
            Alpha.Update(AlphaRate, Samples.Count());
            results[0] /= Samples.Count();
            results[1] /= Samples.Count();

            write("Run Error : " + results[0]);
            write("Run Accuracy : " + results[1]);
            net.Save();
            string Folder = "NeuralNets".GetMyDocs();
            Alpha.Save(Folder);
            //a.Save();
            return results;
        }
    }
}
