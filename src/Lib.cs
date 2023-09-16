
using System;
using System.CodeDom;
using System.IO;
using System.Windows.Forms;

namespace RhGL;

static class PathLib
{
    public static string GetPathWithoutExtension (string path)
    {
        return Path.Combine (Path.GetDirectoryName (path), Path.GetFileNameWithoutExtension (path));
    }
}


static class StringLib
{
    public static string RemoveArrayDescriptor (string name)
    {
        var i = name.IndexOf ('[');
        if (i < 0) return name;
        return name.Substring (0, i);
    }
}


class Debouncer
{
    public delegate void Handler ();
    readonly Handler _handle;
    readonly Eto.Forms.UITimer _timer = new ();
    
    public Debouncer (Handler handle)
    {
        _handle = handle;
        _timer.Elapsed  += _OnTick;
        _timer.Interval  = 1; //every second
    }

    public void Start ()
    {
        _timer.Stop();
        _timer.Start();
    }

    public void Stop ()
    {
        _timer.Stop();
    }
        
    void _OnTick (object sender, EventArgs e)
    {
        _timer.Stop();
        _handle.Invoke ();
    }
}


interface ILNodeContainer <T> where T : class
{
    LNode <T> LinkedNode { get; }
}

class LNode <T> where T : class
{
    internal T @ref;
    internal LQueue <T>? queue;
    internal LNode <T>? prev;
    internal LNode <T>? next;
    public LNode (T element) { @ref = element; }
};

class LQueue <T> where T : class
{
    LNode <T>? first;
    LNode <T>? last;

    void _Detach (LNode <T> node)
    {
        if (node.prev != null) node.prev.next = node.next;
        if (node.next != null) node.next.prev = node.prev;
        if (first == node) first = node.next;
        if (last  == node) last  = node.prev;
        node.prev = node.next = null;
    }

    public void Add (ILNodeContainer <T> element)
    {
        var node = element.LinkedNode;
        if (node.queue != null)
        {
            if (node.queue == this)
                return;
            node.queue._Detach (node);
        }
        first ??= node;
        node.prev = last;
        last = node;
    }

    public T? Pop ()
    {
        var node = first;
        if (node == null) return null;
        _Detach (node);
        return node.@ref;
    }
}