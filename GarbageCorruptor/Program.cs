using System.Linq.Expressions;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GarbageCorruptor
{
    internal class Program
    {
        static unsafe void Main(string[] args)
        {
            string str = "Hello world!";
            CopyString(str, "GRAAAAAAAAAHHHHHHHHHHH");
            Console.WriteLine(str); // GRAAAAAAAAAHHHHHHHHHHH

            Array array = AllocateArray(1, typeof(void).MakeArrayType());
            Console.WriteLine(array.Length); // 1
            Console.WriteLine(array); // System.Void[]

            ResizeArray(int.MaxValue, array);
            Console.WriteLine(array.Length); // 2147483647

            GC.Collect(); // Fatal error. Internal CLR error. (0x80131506)
        }

        public static unsafe void CopyString(string str1, string str2)
        {
            *(int*)Unsafe.AsPointer(ref Unsafe.As<RelaxedGrossObject>(str1).Data) = str2.Length;
            char* chptr = (char*)Unsafe.AsPointer(ref Unsafe.As<RelaxedGrossObject>(str1).Data) + 2;

            for (int i = 0; i < str2.Length; i++)
                chptr[i] = str2[i];
        }

        public static unsafe dynamic AllocateArray(int length, Type type)
        {
            if (!type.HasElementType)
                return null!;

            byte[] bytes = new byte[Marshal.SizeOf(type.GetElementType()!) * length];
            ResizeArray(length, bytes);
            *((nint*)Unsafe.AsPointer(ref Unsafe.As<RelaxedGrossObject>(bytes).Data) - 1) = type.TypeHandle.Value;

            return bytes;
        }

        public static unsafe dynamic ResizeArray(int length, Array array)
        {
            *(int*)Unsafe.AsPointer(ref Unsafe.As<RelaxedGrossObject>(array).Data) = length;
            return array;
        }

        sealed unsafe class RelaxedGrossObject
        {
            public byte Data; // m_data
        }
    }
}