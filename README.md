# GarbageCorruptor
>[!WARNING]
>This project intentionally will corrupt the GC heap, but very optimally.
>
>Do not actually try to do anything done here, or use this code.

This is done by allocating an array with any length, doesn't matter, getting the heap pointer of the array using `Unsafe.AsPointer(ref Unsafe.As<RelaxedGrossObject>(arr).Data)`, and then illegally modifying the length field of the array to `int.MaxValue`.

When the GC next collects, it will try to collect the array, but since we set the length to be `int.MaxValue`, it has to collect the entire process memory.

This instantly crashes the program :)
