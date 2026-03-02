namespace TimeServerTools;

public readonly record struct TimeResult(DateTime CurrentTime, string Timezone, bool isLocal, bool isDST)
{
    public override string ToString()
    {
        return $"Current Time: {CurrentTime}, Timezone: {Timezone}, Is Local: {isLocal}, Is DST: {isDST}";
    }
}