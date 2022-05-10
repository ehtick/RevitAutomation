using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CC_Library.Datatypes;
using System;
using CC_Library.Predictions;

/// <summary>
/// step 1 locate trigram
/// step 2 pull values for Query from trigram (can be a NN or a single layer)
///     This will be set up as a NN that has a couple layers (max)
/// step 3 pull values for Keys from each trigram (can be a NN or a single layer)
///     This will be set up as a NN that has a couple layers (max)
/// step 4 pull values for Values from each trigram (can be a NN or a single layer)
///     This will be set up as a NN that has a couple layers (max)
/// step 5 dot product the Query and the Transpose of the Keys
///     This will be accomplished by multiplying the Query Result with each of the key results across all sets rather than as a single dot product.
/// step 6 softmax each index in the output
/// step 7 multiply the softmax score by the values for the trigram.
/// </summary>

namespace CC_Library.Predictions
{
    [Serializable]
    internal class AlphaFilter4 : IAlphaFilter
    {
        public NeuralNetwork[] Networks { get; }
        private const int Radius = 2;
        private const int Size = 400;
        private const double dropout = 0.1;
        private const double ChangeSize = 0.1;
        internal AlphaFilter4(WriteToCMDLine write)
        {
            this.Networks = new NeuralNetwork[3];
            Networks[0] = new NeuralNetwork(Datatype.Alpha);
            Networks[1] = new NeuralNetwork(Datatype.Alpha);
            Networks[2] = new NeuralNetwork(Datatype.Alpha);
            Networks[0].Layers.Add(new Layer(200, 1 + (CharSet.CharCount * (1 + (2 * Radius))), Activation.LRelu, 1e-5, 1e-5));
            Networks[0].Layers.Add(new Layer(Size, Networks[0].Layers.Last(), Activation.LRelu, 1e-5, 1e-5));
            Networks[1].Layers.Add(new Layer(200, 1 + (CharSet.CharCount * (1 + (2 * Radius))), Activation.LRelu, 1e-5, 1e-5));
            //Networks[1].Layers.Add(new Layer(500, Networks[0].Layers.Last(), Activation.LRelu, 1e-5, 1e-5));
            //Networks[1].Layers.Add(new Layer(500, Networks[0].Layers.Last(), Activation.LRelu, 1e-5, 1e-5));
            Networks[1].Layers.Add(new Layer(Size, Networks[0].Layers.Last(), Activation.LRelu, 1e-5, 1e-5));
            Networks[2].Layers.Add(new Layer(200, 1 + (CharSet.CharCount * (1 + (2 * Radius))), Activation.LRelu, 1e-5, 1e-5));
            //Networks[2].Layers.Add(new Layer(500, Networks[0].Layers.Last(), Activation.LRelu, 1e-5, 1e-5));
            //Networks[2].Layers.Add(new Layer(500, Networks[0].Layers.Last(), Activation.LRelu, 1e-5, 1e-5));
            Networks[2].Layers.Add(new Layer(Size, Networks[0].Layers.Last(), Activation.LRelu, 1e-5, 1e-5));
        }
        public string Name { get { return "AlphaFilterv4"; } }
        public int GetSize() { return Size; }
        public double GetChangeSize() { return ChangeSize; }
        public double[][][][][] Forward(string s)
        {
            double[][][][][] output = new double[4][][][][];
            output[0] = new double[s.Length][][][];
            output[1] = new double[s.Length][][][];
            output[2] = new double[s.Length][][][];
            output[3] = new double[s.Length][][][];

            Parallel.For(0, s.Length, j =>
            {
                output[0][j] = new double[Networks[0].Layers.Count() + 1][][];
                output[0][j][0] = new double[2][];
                output[0][j][0][0] = s.Locate(j, Radius);
                output[0][j][0][1] = output[0][j][0][0];
                for (int i = 0; i < Networks[0].Layers.Count(); i++)
                {
                    output[0][j][i + 1] = new double[2][];
                    try { output[0][j][i + 1][0] = Networks[0].Layers[i].Output(output[0][j][i][1]); }
                    catch { Console.WriteLine("Failed at Network 0 Layer : " + i + ", inputs : " + output[0][j][i][1].Count() + ", weights : " +
                        Networks[0].Layers[i].Weights.GetLength(0) + ", " + Networks[0].Layers[i].Weights.GetLength(1)); }
                    output[0][j][i + 1][1] = Networks[0].Layers[i].DropOut(output[0][j][i + 1][0], dropout);
                }

                output[1][j] = new double[Networks[1].Layers.Count() + 1][][];
                output[1][j][0] = new double[2][];
                output[1][j][0][0] = s.Locate(j, Radius);
                output[1][j][0][1] = output[1][j][0][0];
                for (int i = 0; i < Networks[1].Layers.Count(); i++)
                {
                    output[1][j][i + 1] = new double[2][];
                    try { output[1][j][i + 1][0] = Networks[1].Layers[i].Output(output[1][j][i][1]); }
                    catch {
                        Console.WriteLine("Failed at Network 1 Layer : " + i + ", inputs : " + output[1][j][i][1].Count() + ", weights : " +
                    Networks[1].Layers[i].Weights.GetLength(0) + ", " + Networks[1].Layers[i].Weights.GetLength(1)); }
                    output[1][j][i + 1][1] = Networks[1].Layers[i].DropOut(output[1][j][i + 1][0], dropout);
                }

                output[2][j] = new double[Networks[2].Layers.Count() + 1][][];
                output[2][j][0] = new double[2][];
                output[2][j][0][0] = s.Locate(2, Radius);
                output[2][j][0][1] = output[2][j][0][0];
                for (int i = 0; i < Networks[2].Layers.Count(); i++)
                {
                    output[2][j][i + 1] = new double[2][];
                    try { output[2][j][i + 1][0] = Networks[2].Layers[i].Output(output[2][j][i][1]); }
                    catch
                    {
                        Console.WriteLine("Failed at Network 1 Layer : " + i + ", inputs : " + output[2][j][i][1].Count() + ", weights : " +
                    Networks[2].Layers[i].Weights.GetLength(0) + ", " + Networks[2].Layers[i].Weights.GetLength(1));
                    }
                    output[2][j][i + 1][1] = Networks[1].Layers[i].DropOut(output[2][j][i + 1][0], dropout);
                }
            });
            Parallel.For(0, s.Length, j =>
            {
                output[3][j] = new double[1][][];
                output[3][j][0] = new double[1][];
                output[3][j][0][0] = new double[Size];
                Parallel.For(0, Size, k =>
                {
                    output[3][j][k]
                });
            });
            /*
            output[2][0][0][1] = Activations.SoftMax(output[2][0][0][0]);
            Parallel.For(0, Size, j =>
            {
                for (int i = 0; i < s.Length; i++)
                {
                    output[2][0][0][2][j] += output[0][i][Networks[0].Layers.Count()][0][j] * output[2][0][0][1][i];
                }
            });
            */
            return output;
        }
        public void Backward
            (double[] DValues, double[][][][][] outputs, NetworkMem[] mem, WriteToCMDLine write, bool tf = false)
        {
            var ContextualDVals = new double[outputs[0].Count()]; //output[0].Count() => S.Length
            for(int i = 0; i < ContextualDVals.Count(); i++)
            {
                for(int j = 0; j < DValues.Count(); j++) //DValues.Count() => Size
                {
                    ContextualDVals[i] += DValues[j] * outputs[0][i][Networks[0].Layers.Count()][1][j];
                }
            }
            ContextualDVals = Activations.InverseSoftMax(ContextualDVals, outputs[2][0][0][0]);
            Parallel.For(0, outputs[0].Count(), j =>
            {
                try
                {
                    double[] LocalDVals = new double[Size];
                    for(int i = 0; i < Size; i++) { LocalDVals[i] = DValues[i] * outputs[2][0][0][1][j]; }
                    for (int i = Networks[0].Layers.Count() - 1; i >= 0; i--)
                    {
                        LocalDVals = Networks[0].Layers[i].InverseDropOut(LocalDVals, outputs[0][j][i+1][1]);
                        LocalDVals = mem[0].Layers[i].DActivation(LocalDVals, outputs[0][j][i + 1][0]);
                        mem[0].Layers[i].DBiases(LocalDVals, Networks[0].Layers[i], outputs[0].Count());
                        mem[0].Layers[i].DWeights(LocalDVals, outputs[0][j][i][1], Networks[0].Layers[i], outputs[0].Count());
                        LocalDVals = mem[0].Layers[i].DInputs(LocalDVals, Networks[0].Layers[i]);
                    }
                    double[] cdv = new double[1] { ContextualDVals[j] / outputs[0].Count() };
                    for (int i = Networks[1].Layers.Count() - 1; i >= 0; i--)
                    {
                        cdv = Networks[1].Layers[i].InverseDropOut(cdv, outputs[1][j][i+1][1]);
                        cdv = mem[1].Layers[i].DActivation(cdv, outputs[1][j][i+1][0]);
                        mem[1].Layers[i].DBiases(cdv, Networks[1].Layers[i], outputs[0].Count());
                        mem[1].Layers[i].DWeights(cdv, outputs[1][j][i][1], Networks[1].Layers[i], outputs[0].Count());
                        cdv = mem[1].Layers[i].DInputs(cdv, Networks[1].Layers[i]);
                    }
                }
                catch (Exception e) { e.OutputError(); }
            });
        }
    }
}