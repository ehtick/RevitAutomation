﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC_Library.Predictions
{
    /// <summary>
    /// index of stonk in enum [enum size]
    /// % change of stonk (price at close - price at open) / price at close
    /// variability ratio (high - low) / price at close
    /// secondary ratio (price at close - vwap / price at close
    /// hours since change
    /// Volume
    /// </summary>
    [Serializable]
    public class StonkValues
    {
        public Guid id { get; }
        public string Symbol { get; }
        public DateTime Time { get; }
        public double AskPrice { get; set; }
        public double AskSize { get; set; }
        public double BidPrice { get; set; }
        public double BidSize { get; set; }
        public StonkValues(string symbol, DateTime dt, double askprice, double asksize, double bidprice, double bidsize)
        {
            this.id = new Guid();
            this.Symbol = symbol;
            this.Time = dt;
            this.AskPrice = askprice;
            this.AskSize = asksize;
            this.BidPrice = bidprice;
            this.BidSize = bidsize;
        }
        public double[] Coordinate(StonkValues v1)
        {
            double[] vals = new double[9];
            vals[0] = Symbol == "AAPL" ? 1 : 0;
            vals[1] = Symbol = "QQQ" ? 1 : 0;
            vals[2] = Symbol = "VTI" ? 1 : 0;
            vals[3] = (this.Time - v1.Time).TotalHours;
            vals[4] = v1.Time.Hours / 24.0;
            vals[5] = (this.AskPrice - v1.AskPrice) / v1.AskPrice;
            vals[6] = (this.AskSize - v1.AskSize) / v1.AskSize;
            vals[7] = (this.BidPrice - v1.BidPrice) / v1.BidPrice;
            vals[8] = (this.BidSize - v1.BidSize) / v1.BidSize;
            return vals;
        }
        public void Save()
        {
            string folder = "StockValues".GetMyDocs();
            if(!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            string subfolder = folder + "\\" + Symbol;
            if(!Directory.Exists(subfolder))
                Directory.CreateDirectory(subfolder);
            string fn = subfolder + "\\" + Symbol + "_" + Time.ToString("yyyyMMddhhmmss") + ".bin";
            fn.WriteToBinaryFile<StonkValues>(this);            
        }
    }
}
