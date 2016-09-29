using System;

namespace NSubstitute.Weaver.Tests.Hackweek
{
    public interface IExperiment
    {
        string A { get; }
        string B { get; }
    }

    public class Experiment : IExperiment
    {
        public string A => "a";
        public string B => "b";
    }
}
