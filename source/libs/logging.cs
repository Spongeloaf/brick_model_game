using Godot;


// Logs to visual studio debug output and godot console
public static class OmniLogger
{
    public static void Info(string message)
    {
        GD.Print(message);
        System.Diagnostics.Debug.WriteLine(message);
    }

    public static void Error(string message)
    {
        GD.PrintErr(message);
        System.Diagnostics.Debug.WriteLine(message);
    }
}

