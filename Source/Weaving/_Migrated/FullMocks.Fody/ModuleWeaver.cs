using System;
using System.IO;
using CallSitePatcher;
using Mono.Cecil;
using WrapMscorlib2;

public class ModuleWeaver
{
    public Action<string> LogInfo { get; set; }
    // An instance of Mono.Cecil.ModuleDefinition for processing
    public ModuleDefinition ModuleDefinition { get; set; }

    public string ProjectDirectoryPath { get; set; }
    public string AssemblyFilePath { get; set; }

	string _projectRoot, _projectOutputRoot, _slnRoot, _nsubPath;

    public void Execute()
    {
		_projectRoot = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(AssemblyFilePath), "../.."));
	    _projectOutputRoot = Path.Combine(_projectRoot, "bin/Debug");
		_slnRoot = Path.GetFullPath(Path.Combine(_projectRoot, ".."));
		_nsubPath = Path.GetFullPath(Path.Combine(_slnRoot, "../../External/NSubstitute/NET40/NSubstitute.dll"));

		//RunPrologPatcher(Path.GetFullPath(Path.Combine(_slnRoot, "../../build/CombinedAssemblies/UnityEditor.dll")));
		RunPrologPatcher(Path.GetFullPath(Path.Combine(_slnRoot, "../../build/CombinedAssemblies/UnityEngine.dll")));
		RunPrologPatcher(Path.Combine(_slnRoot, "Engine/bin/Debug/Engine.dll"));

        var p = WrapMscorelib();
        PatchTestAssembly(p);
    }

    void PatchTestAssembly(string s)
    {
        var fakeAssembly = AssemblyDefinition.ReadAssembly(s);
        var assembly = ModuleDefinition.Assembly;

        var patcher = new Patcher();
        patcher.Patch(fakeAssembly, assembly, null);
    }

	void RunPrologPatcher(string sourcePath)
	{
		var now = DateTime.Now;

		if (!File.Exists(sourcePath))
			throw new FileNotFoundException(sourcePath);

		var filename = Path.GetFileName(sourcePath);
		var root = Path.GetDirectoryName(sourcePath);
		var patchedPath = Path.Combine(root, "Patched", filename);

		LogInfo($"Rewriting {sourcePath} to {patchedPath} using {_nsubPath}");

        Unity.Testing.PrologPatcher.Program.Patch(sourcePath, _nsubPath);

		var targetPath = Path.Combine(_projectOutputRoot, filename);
		File.Copy(patchedPath, targetPath, true);

		LogInfo($"...to {targetPath} in {(DateTime.Now - now).TotalSeconds}s");
	}

    string WrapMscorelib()
    {
		var mscorlibPath = typeof(object).Assembly.Location;
        var realtargetPath = Path.Combine(Path.GetDirectoryName(AssemblyFilePath), "../../bin/Debug/");
        var unityBasePath = Path.Combine(Path.GetDirectoryName(AssemblyFilePath), "../../../Engine/bin/Debug");
		var nsubPath = Path.Combine(Path.GetDirectoryName(AssemblyFilePath), "../../../../../External/NSubstitute/NET40/NSubstitute.dll");
        var targetPath = Path.Combine( realtargetPath, "mscorlib.fake.dll" );
        
        LogInfo($"Copying {mscorlibPath} to {targetPath}");
        
        var assembly = Wrapper.Wrap(mscorlibPath, nsubPath);
            
        assembly.Write(targetPath);
      
        return targetPath;
    }
}
