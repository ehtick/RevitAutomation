﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using CC_Library.Datatypes;
using System.Runtime.InteropServices;

namespace CC_Library.Predictions
{
    public static class MasterformatNetwork
    {
        private const double dropout = 0.1;
        public static Datatype datatype { get { return Datatype.Masterformat; } }
        public static NeuralNetwork GetNetwork(WriteToCMDLine write)
        {
            Alpha2 a = new Alpha2(CMDLibrary.WriteNull);
            NeuralNetwork net = datatype.LoadNetwork(write);
            if (net.Datatype == Datatype.None)
            {
                net = new NeuralNetwork(datatype);
                net.Layers.Add(new Layer(30, a.GetSize(), Activation.LRelu, 1e-5, 1e-5));
                net.Layers.Add(new Layer(30, net.Layers.Last(), Activation.LRelu, 1e-5, 1e-5));
                net.Layers.Add(new Layer(40, net.Layers.Last(), Activation.CombinedCrossEntropySoftmax));
            }
            return net;
        }
        public static double[] Predict(string s, WriteToCMDLine write)
        {
            NeuralNetwork net = GetNetwork(write);
            Alpha2 a = datatype.LoadAlpha(write);
            var mem = a.CreateAlphaMemory(s);
            double[] Results = a.Forward(s, write).Key;
            Results.WriteArray("Alpha Results : ", write);
            for(int i = 0; i < net.Layers.Count(); i++)
            {
                Results = net.Layers[i].Output(Results);
            }
            return Results;
        }
        public static double[] Propogate
            (Sample s, WriteToCMDLine write, bool tf = false)
        {
            var results = new double[2];
            NeuralNetwork net = GetNetwork(write);
            var Samples = s.ReadSamples(24);
            Alpha2 a = datatype.LoadAlpha(write);
            var am = a.CreateMemory();
            NeuralNetwork DictNet = Predictionary.GetNetwork(write);
            //Alpha a = new Alpha(write);
            //AlphaContext ctxt = new AlphaContext(datatype, write);
            NetworkMem MFMem = new NetworkMem(net);

            try
            {
                Parallel.For(0, Samples.Count(), j =>
                {
                    //AlphaMem am = new AlphaMem(Samples[j].TextInput.ToCharArray());
                    //var output = a.Forward(Samples[j].TextInput, ctxt, am);
                    var AMem = a.CreateAlphaMemory(Samples[j].TextInput, DictNet);
                    var output = a.Forward(Samples[j].TextInput, write, DictNet);
                    var F = net.Forward(output.Key, dropout, write);
                    if (j == 0)
                    {
                        F.Last().GetRank(0).WriteArray("Output[0]", write);
                        F.Last().GetRank(1).WriteArray("Output[1]", write);
                        Samples[j].DesiredOutput.WriteArray("Desired", write);
                    }
                    results[0] += CategoricalCrossEntropy.Forward(F.Last().GetRank(0), Samples[j].DesiredOutput).Max();
                    results[1] += F.Last().GetRank(0).ToList().IndexOf(F.Last().GetRank(0).Max()) ==
                        Samples[j].DesiredOutput.ToList().IndexOf(Samples[j].DesiredOutput.Max()) ? 1 : 0;

                    var DValues = net.Backward(F, Samples[j].DesiredOutput, MFMem, write);
                    a.Backward(DValues, output.Value, am, write, DictNet);
                    //a.Backward(Samples[j].TextInput, DValues, ctxt, am, AlphaMem, CtxtMem);
                });
            }
            catch (Exception e) { e.OutputError(); }
            MFMem.Update(Samples.Count(), 1e-4, net);
            a.Update(am, Samples.Count());
            //AlphaMem.Update(Samples.Count(), 0.00001, a.Network);
            //CtxtMem.Update(Samples.Count(), 0.00001, ctxt.Network);
            results[0] /= Samples.Count();
            results[1] /= Samples.Count();
            net.Save();
            a.Save();

            //s.Save();
            return results;
        }
    }
}