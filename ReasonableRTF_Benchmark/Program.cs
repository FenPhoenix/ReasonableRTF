using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using ReasonableRTF;

namespace ReasonableRTF_Benchmark;

public class Test
{
    // Believe it or not you have to put your exact actual path here because BenchmarkDotNet causes every kind of
    // app startup path getting method in the ENTIRE UNIVERSE to not work. Argh.
    private const string TestDataDir = @"C:\Users\Brian\source\repos\ReasonableRTF\ReasonableRTF_TestApp\Data";

    private readonly MemoryStream[] _fullSetMemStreams;
    private readonly MemoryStream[] _smallSetMemStreams;
    private readonly byte[][] _fullSetByteArrays;
    private readonly byte[][] _smallSetByteArrays;
    private readonly RichTextBox _rtfBox;
    private readonly RtfToTextConverter _rtfConverter;

    private MemoryStream[] GetStuff_RichTextBox(bool small)
    {
        string[] rtfFiles = Directory.GetFiles(GetRtfSetDir(small));
        MemoryStream[] memStreams = new MemoryStream[rtfFiles.Length];

        for (int i = 0; i < rtfFiles.Length; i++)
        {
            string f = rtfFiles[i];
            using var fs = File.OpenRead(f);
            byte[] array = new byte[fs.Length];
            fs.ReadExactly(array, 0, (int)fs.Length);
            memStreams[i] = new MemoryStream(array);
        }

        return memStreams;
    }

    private byte[][] GetStuff_Custom(bool small)
    {
        string[] rtfFiles = Directory.GetFiles(GetRtfSetDir(small));

        byte[][] byteArrays = new byte[rtfFiles.Length][];

        for (int i = 0; i < rtfFiles.Length; i++)
        {
            string f = rtfFiles[i];
            using var fs = File.OpenRead(f);
            byte[] array = new byte[fs.Length];
            fs.ReadExactly(array, 0, (int)fs.Length);
            byteArrays[i] = array;
        }

        return byteArrays;
    }

    public Test()
    {
        _fullSetMemStreams = GetStuff_RichTextBox(small: false);
        _smallSetMemStreams = GetStuff_RichTextBox(small: true);

        _fullSetByteArrays = GetStuff_Custom(small: false);
        _smallSetByteArrays = GetStuff_Custom(small: true);

        _rtfBox = new RichTextBox();
        _rtfConverter = new RtfToTextConverter();
    }

    private const string _rtfFullSetDir = "RTF_Test_Set_Full";
    private const string _rtfSmallSetDir = "RTF_Test_Set_Small";

    private string GetRtfSetDir(bool small)
    {
        string dir = small ? _rtfSmallSetDir : _rtfFullSetDir;
        return Path.Combine(TestDataDir, dir);
    }

    [Benchmark]
    public void FullSet_RichTextBox()
    {
        for (int i = 0; i < _fullSetMemStreams.Length; i++)
        {
            _fullSetMemStreams[i].Position = 0;
            _rtfBox.LoadFile(_fullSetMemStreams[i], RichTextBoxStreamType.RichText);
            _ = _rtfBox.Text;
        }
    }

    [Benchmark]
    public void NoImageSet_RichTextBox()
    {
        for (int i = 0; i < _smallSetMemStreams.Length; i++)
        {
            _smallSetMemStreams[i].Position = 0;
            _rtfBox.LoadFile(_smallSetMemStreams[i], RichTextBoxStreamType.RichText);
            _ = _rtfBox.Text;
        }
    }

    [Benchmark]
    public void FullSet_ReasonableRTF()
    {
        for (int i = 0; i < _fullSetByteArrays.Length; i++)
        {
            _ = _rtfConverter.Convert(_fullSetByteArrays[i]);
        }
    }

    [Benchmark]
    public void NoImageSet_ReasonableRTF()
    {
        for (int i = 0; i < _smallSetByteArrays.Length; i++)
        {
            _ = _rtfConverter.Convert(_fullSetByteArrays[i]);
        }
    }
}

internal static class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("ReasonableRTF Benchmark\r\n" +
                          "-----------------------\r\n");

        Summary summary = BenchmarkRunner.Run<Test>();
    }
}
