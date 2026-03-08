using System;

namespace TimeServerTools;

public readonly record struct TimeResult(DateTime CurrentTime, string Timezone, bool IsLocal, bool IsDST)
{
    public override string ToString()
    {
        return $"Current Time: {CurrentTime}, Timezone: {Timezone}, Is Local: {IsLocal}, Is DST: {IsDST}";
    }
}