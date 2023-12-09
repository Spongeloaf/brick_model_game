using Godot;
using System.Collections.Generic;
using System.IO;

public partial class AnchorGenerator : Node
{
    private List<string> m_anchorFiles = new List<string>();

    public override void _Ready() 
    {
        string customPartsFOlder = ProjectSettings.GlobalizePath("res://models/customParts/");
        string[] files = Directory.GetFiles(customPartsFOlder);
        foreach (string file in files)
        {
            ProcessFile(file);
        }
    }

    private void ProcessFile(string file)
    {
        if (!file.EndsWith("anchor.dat"))
            return;

        if 
    }
}
