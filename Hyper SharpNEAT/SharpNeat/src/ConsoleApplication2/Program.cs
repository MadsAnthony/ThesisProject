using System;
using System.Collections.Generic;
using System.Text;
using SharpNeatLib;
using SharpNeatLib.AppConfig;
using SharpNeatLib.Evolution;
using SharpNeatLib.Evolution.Xml;
using SharpNeatLib.Experiments;
using SharpNeatLib.NeatGenome;
using SharpNeatLib.NeatGenome.Xml;
using SharpNeatLib.NeuralNetwork;
using SharpNeatLib.NeuralNetwork.Xml;
using System.Xml;
using System.IO;
using System.Threading;

namespace SharpNeat.Experiments
{
    class Program
    {

        static void Main(string[] args)
        {

            NeatGenome seedGenome = null;
            string filename = @"seedGenome.xml";
            try
            {
                XmlDocument document = new XmlDocument();
                document.Load(filename);
                seedGenome = XmlNeatGenomeReaderStatic.Read(document);
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Problem loading genome. \n" + e.Message);
            }


            double maxFitness = 0;
            int maxGenerations = 50;
            int populationSize = 30;//100;//150;

            Thread extraWindowThread;
            extraWindowThread = new System.Threading.Thread(delegate()
            {
                var myForm = new SharpNeatExperiments.Pacman.MyForm1();
                System.Windows.Forms.Application.Run(myForm);
            });
            extraWindowThread.Start();
            IExperiment exp = new PacmanExperimentSUPG(4, 12, 12, 4, 2);
            StreamWriter SW;
            SW = File.CreateText("logfile.txt");
            //Change this line for different experiments
            XmlDocument doc;
            FileInfo oFileInfo;
            IdGenerator idgen;
            EvolutionAlgorithm ea;
            if (seedGenome == null)
            {
                idgen = new IdGenerator();
                ea = new EvolutionAlgorithm(new Population(idgen, GenomeFactory.CreateGenomeList(exp.DefaultNeatParameters, idgen, exp.InputNeuronCount, exp.OutputNeuronCount, exp.DefaultNeatParameters.pInitialPopulationInterconnections, populationSize)), exp.PopulationEvaluator, exp.DefaultNeatParameters);

            }
            else
            {
                idgen = new IdGeneratorFactory().CreateIdGenerator(seedGenome);
                ea = new EvolutionAlgorithm(new Population(idgen, GenomeFactory.CreateGenomeList(seedGenome, exp.DefaultNeatParameters.populationSize, exp.DefaultNeatParameters, idgen)), exp.PopulationEvaluator, exp.DefaultNeatParameters);
            }
            bool isNSGAiiEnabled = false;
            for (int j = 0; j < maxGenerations; j++)
            {
                DateTime dt = DateTime.Now;
                if (isNSGAiiEnabled)
                {
                    ea.PerformOneGenerationNSGAii();
                }
                else
                {
                    ea.PerformOneGeneration();
                }
                if (ea.BestGenome.Fitness > maxFitness)
                {
                    maxFitness = ea.BestGenome.Fitness;
                    Console.WriteLine(maxFitness + "maxFitness");
                    Console.WriteLine("objectiveFitness" + ea.BestGenome.MultiObjectiveFitness[0] + " " + ea.BestGenome.MultiObjectiveFitness[1]);
                    doc = new XmlDocument();
                    XmlGenomeWriterStatic.Write(doc, (NeatGenome)ea.BestGenome);
                    oFileInfo = new FileInfo("bestGenome" + j.ToString() + ".xml");
                    doc.Save(oFileInfo.FullName);


                }
                Console.WriteLine(ea.Generation.ToString() + " " + (maxFitness).ToString() + " " + (DateTime.Now.Subtract(dt)));
                //Do any post-hoc stuff here


                SW.WriteLine(ea.Generation.ToString() + " " + (maxFitness).ToString());

            }
            SW.Close();
            //----- Write the genome to an XmlDocument.
            doc = new XmlDocument();
            XmlGenomeWriterStatic.Write(doc, (NeatGenome)ea.BestGenome, ActivationFunctionFactory.GetActivationFunction("NullFn"));
            oFileInfo = new FileInfo("bestGenome.xml");
            doc.Save(oFileInfo.FullName);

        }
    }
}
