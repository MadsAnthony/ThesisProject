using System;
using System.Collections.Generic;
using System.Text;

namespace SharpNeatLib.NeuralNetwork
{
    class Cosine : IActivationFunction
    {
        #region IActivationFunction Members

        public double Calculate(double inputSignal)
        {
            return Math.Cos(2*inputSignal);
           
        }

        public float Calculate(float inputSignal)
        {
            return (float)Math.Cos(2*inputSignal);
        }

        public string FunctionId
        {
            get { return this.GetType().Name; }
        }

        public string FunctionString
        {
            get { return "Cos(2*inputSignal)"; }
        }

        public string FunctionDescription
        {
            get { return "Cos function with doubled period"; }
        }

        #endregion
    }
}
