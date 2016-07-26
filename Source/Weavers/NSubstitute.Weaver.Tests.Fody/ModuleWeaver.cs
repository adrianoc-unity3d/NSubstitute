using System;
using Mono.Cecil;
using NSubstitute.Weavers;

// this project exists just to make fody happy for in-solution weaving. note that this project
// outputs to solution root "Weavers" folder for the binaries so fody picks it up in order to
// patch the test project.
//
// https://github.com/Fody/Fody/wiki/InSolutionWeaving

public class ModuleWeaver
{
    readonly FodyWeaver m_FodyWeaver = new FodyWeaver();

    public ModuleDefinition ModuleDefinition { get { return m_FodyWeaver.ModuleDefinition; } set { m_FodyWeaver.ModuleDefinition = value; } }
    public Action<string> LogDebug { get { return m_FodyWeaver.LogDebug; } set { m_FodyWeaver.LogDebug = value; } }
    public Action<string> LogInfo { get { return m_FodyWeaver.LogInfo; } set { m_FodyWeaver.LogInfo = value; } }
    public Action<string> LogWarning { get { return m_FodyWeaver.LogWarning; } set { m_FodyWeaver.LogWarning = value; } }
    public Action<string> LogError { get { return m_FodyWeaver.LogError; } set { m_FodyWeaver.LogError = value; } }

    public void Execute()
    {
        m_FodyWeaver.Execute();
    }
}
