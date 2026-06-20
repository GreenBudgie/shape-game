#if TOOLS
[Tool]
public partial class PaddingUndoRedoAction : GodotObject
{
    private string _path = "";
    private Image _doImage = null!;
    private Image _undoImage = null!;

    public void Init(string path, Image doImage, Image undoImage)
    {
        _path = path;
        _doImage = doImage;
        _undoImage = undoImage;
    }

    public void Do()
    {
        _doImage.SavePng(_path);
        Reimport(_path);
        GD.Print($"[do]   {_path}");
    }

    public void Undo()
    {
        _undoImage.SavePng(_path);
        Reimport(_path);
        GD.Print($"[undo] {_path}");
    }

    private static void Reimport(string path)
    {
        var fs = EditorInterface.Singleton.GetResourceFilesystem();
        fs.UpdateFile(path);
        fs.ReimportFiles([path]);
        fs.Scan();
    }
}
#endif