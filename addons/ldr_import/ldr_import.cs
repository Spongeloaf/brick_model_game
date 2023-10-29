using Godot;
using System;


public partial class ldr_import : EditorImportPlugin
{
  public override string _GetImporterName()
  {
    return "Ldr.Importer";
  }

  public override string _GetVisibleName()
  {
    return "Ldr Importer";
  }

  public override string[] _GetRecognizedExtensions()
  {
    return new string[] {".ldr"};
  }

  public override string _GetSaveExtension()
  {
    return "ldr_scn";
  }

  public override string _GetResourceType()
  {
    return "Node3D";
  }

  public override int _GetPresetCount()
  {
    return 1;
  }

  public override string _GetPresetName(int presetIndex)
  {
    return "Default";
  }

  public override Error _Import(string sourceFile, string savePath, Godot.Collections.Dictionary options, Godot.Collections.Array<string> platformVariants, Godot.Collections.Array<string> genFiles)
  {
    using var file = FileAccess.Open(sourceFile, FileAccess.ModeFlags.Read);
    if (file.GetError() != Error.Ok)
      return Error.Failed;

    // hook up to ldr files here

    return Error.Ok;
  }

  //public override Godot.Collections.Array<Godot.Collections.Dictionary> _GetImportOptions(string path, int presetIndex)
  //{
  //  return new Godot.Collections.Array<Godot.Collections.Dictionary>
  //      {
  //          new Godot.Collections.Dictionary
  //          {
  //              { "name", "myOption" },
  //              { "default_value", false },
  //          }
  //      };
  //}
}
