using Godot;
using Ldraw;
using System.Collections.Generic;

public partial class Loader : Node3D
{
    [Export] public string _BasePartsPath = "C:\\dev\\ldraw\\";
    [Export] private string _ModelsPath = "res://models/imports/";
    [Export] private string _MaterialsPath = "res://assets/generated/materials/";
    [Export] private string _MeshesPath = "res://assets/generated/meshes/";
    [Export] private string _ScenesPath = "res://assets/generated/scenes/";
    [Export] private string _ColorConfigPath;
    [Export] private float _Scale;
    [Export] private BaseMaterial3D _DefaultOpaqueMaterial;
    [Export] private BaseMaterial3D _DefaultTransparentMaterial;
    public const int DefaultMaterialCode = 16;

    public override void _Ready()
    {
        _ColorConfigPath = _BasePartsPath + "LDConfig.ldr";
        _DefaultOpaqueMaterial = ResourceLoader.Load<BaseMaterial3D>("res://assets/materials/importDefaults/DefaultOpaque.tres");
        _DefaultTransparentMaterial = ResourceLoader.Load<BaseMaterial3D>("res://assets/materials/importDefaults/DefaultTransparent.tres");

        //Node3D modelScene = LoadModel("C:\\dev\\brick_model_game\\models\\imports\\prop_model.mpd");
        //modelScene.Name = "prop_model";

        Node3D modelScene = LoadModelsFromFile("C:\\dev\\brick_model_game\\models\\imports\\prop.mpd");

        Error error = SaveNodeAsScene(modelScene);
        if (error != Error.Ok)
            GD.PrintErr("Error saving scene: " + error.ToString());
        GetTree().Quit();
    }

    private Node3D GetPartMesh(string partFile)
    {
        MeshManager meshManager = new MeshManager();
        Command command = new Command();
        command.subfileName = partFile;
        Primitive.AddPrimitiveToMesh(meshManager, in command);
        return meshManager.GetMeshInstance();
    }

    // Returns a Node3D with each model in the file as a child
    private Node3D LoadModelsFromFile(string modelfile)
    {
        UserModelFile file = new UserModelFile(modelfile);
        return file.GetModels();
    }

    private Node3D LoadModel(string modelfile)
    {
        //Command command = new Command();
        //command.metadata.fileName = modelfile;
        //command.subfileName = modelfile;
        //command.type = CommandType.Model;
        //command.modelType = ModelTree.ModelTypes.prop;
        //Model model = new Model(command);
        //return model.GetModelInstance();
        return null;
    }

    public Error SaveNodeAsScene(Node3D node)
    {
        if (node == null)
            return Error.InvalidData;

        PackedScene scene = new PackedScene();
        scene.Pack(node);

        //string path = "";
        //if (OS.HasFeature("editor"))
        //{

        //  // Running from an editor binary.
        //  // `path` will contain the absolute path to `hello.txt` located in the project root.
        //  path = ProjectSettings.GlobalizePath(_ScenesPath + node.Name + ".tscn");
        //}
        //else
        //{
        //  // Running from an exported project.
        //  // `path` will contain the absolute path to `hello.txt` next to the executable.
        //  // This is *not* identical to using `ProjectSettings.globalize_path()` with
        //  //a `res://` path, but is close enough in spirit.
        //  path = OS.GetExecutablePath().GetBaseDir().PathJoin(node.Name + ".tscn");
        //}

        string path = "C:\\dev\\brick_model_game\\assets\\generated\\models\\" + node.Name + ".tscn";
        return ResourceSaver.Save(scene, path);
    }
}