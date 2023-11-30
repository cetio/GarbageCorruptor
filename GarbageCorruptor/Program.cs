using System.Runtime.CompilerServices;

namespace GarbageCorruptor
{
    internal class Program
    {
        static unsafe void Main(string[] args)
        {
            int[] arr = new int[1];
            *(int*)Unsafe.AsPointer(ref Unsafe.As<RelaxedGrossObject>(arr).Data) = int.MaxValue;
            GC.Collect();
        }

        sealed unsafe class RelaxedGrossObject
        {
            public byte Data; // m_data
        }
    }
}