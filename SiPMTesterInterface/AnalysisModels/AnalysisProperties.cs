using System;
namespace SiPMTesterInterface.AnalysisModels
{
    public struct AnalysisProperties
    {
        public int nPreSmooth = 1;
        public int preSmoothWidth = 5;
        public int nlnSmooth = 1;
        public int lnSmoothWidth = 5;
        public int nDerivativeSmooth = 1;
        public int derivativeSmoothWidth = 5;
        public int fitWidth = 100;

        //set default properties
        public AnalysisProperties()
        {
            nPreSmooth = 1;
            preSmoothWidth = 5;
            nlnSmooth = 1;
            lnSmoothWidth = 5;
            nDerivativeSmooth = 1;
            derivativeSmoothWidth = 5;
            fitWidth = 100;
        }
    }
}

