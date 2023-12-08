using Godot;
using Ldraw;

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
        ConfigureEnvironment();
        Node3D modelScene = LoadModelsFromFile("C:\\dev\\brick_model_game\\models\\imports\\prop_components.mpd");

        Error error = SaveNodeAsScene(modelScene);
        if (error != Error.Ok)
            GD.PrintErr("Error saving scene: " + error.ToString());
        GetTree().Quit();
    }

    private void ConfigureEnvironment()
    {
        _ColorConfigPath = _BasePartsPath + "LDConfig.ldr";
        _DefaultOpaqueMaterial = ResourceLoader.Load<BaseMaterial3D>("res://assets/materials/importDefaults/DefaultOpaque.tres");
        _DefaultTransparentMaterial = ResourceLoader.Load<BaseMaterial3D>("res://assets/materials/importDefaults/DefaultTransparent.tres");
        MaterialManager.Configure(_ColorConfigPath, _DefaultOpaqueMaterial, _DefaultTransparentMaterial);
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


    public Error SaveNodeAsScene(Node3D node)
    {
        if (node == null)
            return Error.InvalidData;

        PackedScene scene = new PackedScene();
        scene.Pack(node);
        string path = "C:\\dev\\brick_model_game\\assets\\generated\\models\\" + node.Name + ".tscn";
        return ResourceSaver.Save(scene, path);
    }
}