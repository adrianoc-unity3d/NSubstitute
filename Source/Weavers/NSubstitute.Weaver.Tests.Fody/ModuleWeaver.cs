using System;
using System.IO;
using Mono.Cecil;
using RealModuleWeaver = NSubstitute.Weavers.Fody.ModuleWeaver;

// this project exists just to make fody happy for in-solution weaving. note that this project
// outputs to solution root "Weavers" folder for the binaries so fody picks it up in order to
// patch the test project.
//
// https://github.com/Fody/Fody/wiki/InSolutionWeaving

public class ModuleWeaver
{
    readonly RealModuleWeaver m_RealModuleWeaver = new RealModuleWeaver();
    string m_AddinDirectoryPath;

    // see https://github.com/Fody/Fody/wiki/ModuleWeaver for all requirements and magic properties on this class

    public ModuleDefinition ModuleDefinition { get { return m_RealModuleWeaver.ModuleDefinition; } set { m_RealModuleWeaver.ModuleDefinition = value; } }
    public Action<string> LogDebug { get { return m_RealModuleWeaver.LogDebug; } set { m_RealModuleWeaver.LogDebug = value; } }
    public Action<string> LogInfo { get { return m_RealModuleWeaver.LogInfo; } set { m_RealModuleWeaver.LogInfo = value; } }
    public Action<string> LogWarning { get { return m_RealModuleWeaver.LogWarning; } set { m_RealModuleWeaver.LogWarning = value; } }
    public Action<string> LogError { get { return m_RealModuleWeaver.LogError; } set { m_RealModuleWeaver.LogError = value; } }

    // special: we have a hard coded copy-local msbuild ref to NSubstitute.dll specifically to give to the real weaver. needed to support in-solution weaver tests.
    public string AddinDirectoryPath
    {
        get { return m_AddinDirectoryPath; }
        set
        {
            m_AddinDirectoryPath = value;
            m_RealModuleWeaver.NSubstituteAssemblyPath = Path.Combine(m_AddinDirectoryPath, "NSubstitute.dll");
        }
    }

    public void Execute()
    {
        m_RealModuleWeaver.Execute();
    }
}
