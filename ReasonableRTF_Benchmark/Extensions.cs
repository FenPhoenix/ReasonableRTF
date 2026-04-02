namespace ReasonableRTF_Benchmark;

internal static class Extensions
{
#if NETFRAMEWORK
    internal static void ReadExactly(this Stream stream, byte[] buffer, int offset, int count)
    {
        _ = stream.ReadAll(buffer, offset, count);
    }

    internal static int ReadAll(this Stream stream, byte[] buffer, int offset, int count)
    {
        int bytesReadRet = 0;
        int startPosThisRound = offset;
        while (true)
        {
            int bytesRead = stream.Read(buffer, startPosThisRound, count);
            if (bytesRead <= 0) break;
            bytesReadRet += bytesRead;
            startPosThisRound += bytesRead;
            count -= bytesRead;
        }

        return bytesReadRet;
    }
#endif
}
