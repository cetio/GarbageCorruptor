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

            array = AllocateArrayUninitialized(2, typeof(int).MakeArrayType());
            Console.WriteLine(array.Length); // 2
            Console.WriteLine(array); // System.Int[]

            ResizeArray(int.MaxValue, array);
            Console.WriteLine(array.Length); // 2147483647

            MakeUnapproachable(str);
            MakeUnapproachable(array);

            GC.Collect(); // Fatal error. Internal CLR error. (0x80131506)
        }

        public static unsafe void CopyString(string str1, string str2)
        {
            *(int*)Unsafe.AsPointer(ref Unsafe.As<RelaxedGrossObject>(str1).Data) = str2.Length;
            char* chptr = (char*)Unsafe.AsPointer(ref Unsafe.As<RelaxedGrossObject>(str1).Data) + 2;

            for (int i = 0; i < str2.Length; i++)
                chptr[i] = str2[i];
        }

        public static unsafe void MakeUnapproachable(object obj)
        {
            ((uint*)*((nint*)Unsafe.AsPointer(ref Unsafe.As<RelaxedGrossObject>(obj).Data) - 1))[1] = 4444444;
        }

        public static unsafe dynamic AllocateArray(int length, Type type)
        {
            if (!type.HasElementType)
                return null!;

            byte[] bytes = new byte[Marshal.SizeOf(type.GetElementType()!) * length];
            *((nint*)Unsafe.AsPointer(ref Unsafe.As<RelaxedGrossObject>(bytes).Data) - 1) = type.TypeHandle.Value;
            ResizeArray(length, bytes);

            return bytes;
        }

        public static unsafe dynamic AllocateArrayUninitialized(int length, Type type)
        {
            if (!type.HasElementType)
                return null!;

            object[] array = new object[0];
            *((nint*)Unsafe.AsPointer(ref Unsafe.As<RelaxedGrossObject>(array).Data) - 1) = type.TypeHandle.Value;
            ResizeArray(length, array);

            return array;
        }

        public static unsafe void ResizeArray(int length, Array array)
        {
            *(int*)Unsafe.AsPointer(ref Unsafe.As<RelaxedGrossObject>(array).Data) = length;
        }

        sealed unsafe class RelaxedGrossObject
        {
            public byte Data; // m_data
        }
    }
}