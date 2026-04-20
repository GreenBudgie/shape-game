#if TOOLS
using Godot;

[Tool]
public partial class PaddingPlugin : EditorPlugin
{
    private const int Padding = 8;
    private PaddingContextMenu _contextMenu = null!;

    public override void _EnterTree()
    {
        _contextMenu = new PaddingContextMenu(Padding);
        AddContextMenuPlugin(EditorContextMenuPlugin.ContextMenuSlot.Filesystem, _contextMenu);
    }

    public override void _ExitTree()
    {
        RemoveContextMenuPlugin(_contextMenu);
    }
}

[Tool]
public partial class PaddingContextMenu : EditorContextMenuPlugin
{
    private readonly int _padding;

    public PaddingContextMenu(int padding)
    {
        _padding = padding;
    }

    public override void _PopupMenu(string[] paths)
    {
        // Показываем пункт только если выбран хотя бы один PNG
        var hasPng = false;
        foreach (var path in paths)
        {
            if (path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
            {
                hasPng = true;
                break;
            }
        }

        if (!hasPng) return;

        AddContextMenuItem("Add Padding", Callable.From(() => OnAddPadding(paths)));
    }

    private void OnAddPadding(string[] paths)
    {
        var processed = 0;
        var skipped = 0;

        foreach (var path in paths)
        {
            if (!path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                continue;

            if (ProcessPng(path))
                processed++;
            else
                skipped++;
        }

        GD.Print($"Done! Processed: {processed}, Skipped: {skipped}");
    }

    private bool ProcessPng(string path)
    {
        var image = Image.LoadFromFile(path);
        if (image == null) return false;

        if (image.DetectAlpha() == Image.AlphaMode.None) return false;

        if (HasEnoughPadding(image))
        {
            GD.Print($"[ok]   {path}");
            return false;
        }

        var newImage = Image.Create(
            image.GetWidth() + _padding * 2,
            image.GetHeight() + _padding * 2,
            false,
            Image.Format.Rgba8
        );

        newImage.BlitRect(
            image,
            new Rect2I(0, 0, image.GetWidth(), image.GetHeight()),
            new Vector2I(_padding, _padding)
        );

        newImage.SavePng(path);
        GD.Print($"[done] {path}");
        return true;
    }

    private bool HasEnoughPadding(Image image)
    {
        image.Convert(Image.Format.Rgba8);
        var w = image.GetWidth();
        var h = image.GetHeight();

        for (var x = 0; x < w; x++)
        {
            for (var y = 0; y < _padding; y++)
            {
                if (image.GetPixel(x, y).A > 0) return false;
                if (image.GetPixel(x, h - 1 - y).A > 0) return false;
            }
        }

        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < _padding; x++)
            {
                if (image.GetPixel(x, y).A > 0) return false;
                if (image.GetPixel(w - 1 - x, y).A > 0) return false;
            }
        }

        return true;
    }
}
#endif