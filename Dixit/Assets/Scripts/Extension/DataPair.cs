public class DataPair<T1, T2>
{
    public T1 Value1 { get; set; }
    public T2 Value2 { get; set; }

    public DataPair(T1 v1, T2 v2) 
    {
        Value1 = v1;
        Value2 = v2;
    }
}
